using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRC_SS_Butler;

internal static class Program
{
    private const string DateFolder = "2026-10-19";

    private static async Task<int> Main()
    {
        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine("エラー: Windows上で実行してください。\n詳細: ファイル共有ロックの検証にWindowsが必要です。\n解決方法: Windowsの.NET 8以降で実行してください。");
            return 1;
        }

        var tests = new (string Name, Func<TestContext, Task> Run)[]
        {
            ("直下と日付別サブフォルダーの画像を整理・バックアップ", CreatedFilesAsync),
            ("2秒以上の書き込み完了を待ってから完全な内容をコピー", LockedFileAsync),
            ("一時ファイルからPNGへの名前変更を検出", RenamedFileAsync),
            ("対象外のPNGを無視し、その後も監視を継続", UnrelatedFileAsync),
            ("連続作成した80枚を取りこぼさず整理", BurstAsync),
            ("整理済みの画像と重複通知を安全に処理", AlreadySortedAsync),
            ("監視先内部へのバックアップによる循環を防止", NestedBackupAsync),
            ("ロック待機中でも終了し、終了後の画像を処理しない", ShutdownAsync),
            ("処理エラーを通知し、後続の画像処理を継続", ProcessingErrorAsync),
            ("コピー失敗後は移動済みの画像から再試行", BackupRetryAsync)
        };

        int failures = 0;
        foreach (var test in tests)
        {
            using var context = new TestContext();
            try
            {
                await test.Run(context);
                Console.WriteLine($"成功: {test.Name}");
            }
            catch (Exception ex)
            {
                failures++;
                Console.Error.WriteLine($"エラー: {test.Name}\n詳細: {ex}\n解決方法: 監視処理とテスト条件を確認してください。");
            }
        }
        Console.WriteLine($"結果: {tests.Length - failures}/{tests.Length}件成功");
        return failures == 0 ? 0 : 1;
    }

    private static async Task CreatedFilesAsync(TestContext context)
    {
        context.Start();
        foreach (string directory in new[] { context.Source, Path.Combine(context.Source, "2026-10") })
        {
            Directory.CreateDirectory(directory);
            string name = context.NewName();
            File.WriteAllText(Path.Combine(directory, name), "画像データ");
            await WaitUntilAsync(() => File.Exists(context.BackupFile(name)));
            Assert(File.ReadAllText(context.BackupFile(name)) == "画像データ", "コピー内容が一致しません。");
            Assert(File.Exists(context.SortedFile(name)), "整理先に画像がありません。");
        }
        context.AssertNoErrors();
    }

    private static async Task LockedFileAsync(TestContext context)
    {
        context.Start();
        string name = context.NewName();
        using (var stream = new FileStream(Path.Combine(context.Source, name), FileMode.Create,
            FileAccess.Write, FileShare.ReadWrite | FileShare.Delete))
        {
            stream.Write(Encoding.UTF8.GetBytes("前半"));
            stream.Flush();
            await Task.Delay(2500);
            Assert(!File.Exists(context.SortedFile(name)), "書き込み中の画像が移動されました。");
            Assert(!File.Exists(context.BackupFile(name)), "未完成の画像がコピーされました。");
            stream.Write(Encoding.UTF8.GetBytes("後半"));
        }
        await WaitUntilAsync(() => File.Exists(context.BackupFile(name)));
        Assert(File.ReadAllText(context.BackupFile(name)) == "前半後半", "保存完了後の内容が一致しません。");
        context.AssertNoErrors();
    }

    private static async Task RenamedFileAsync(TestContext context)
    {
        context.Start();
        string name = context.NewName();
        string temporary = Path.Combine(context.Source, "保存中.tmp");
        File.WriteAllText(temporary, "画像データ");
        File.Move(temporary, Path.Combine(context.Source, name));
        await WaitUntilAsync(() => File.Exists(context.BackupFile(name)));
        context.AssertNoErrors();
    }

    private static async Task UnrelatedFileAsync(TestContext context)
    {
        context.Start();
        string unrelated = Path.Combine(context.Source, "対象外.png");
        File.WriteAllText(unrelated, "対象外");
        string name = context.NewName();
        File.WriteAllText(Path.Combine(context.Source, name), "画像データ");
        await WaitUntilAsync(() => File.Exists(context.BackupFile(name)));
        Assert(File.Exists(unrelated), "対象外の画像が移動されました。");
        context.AssertNoErrors();
    }

    private static async Task BurstAsync(TestContext context)
    {
        context.Start();
        string[] names = Enumerable.Range(0, 80).Select(_ => context.NewName()).ToArray();
        foreach (string name in names)
        {
            File.WriteAllText(Path.Combine(context.Source, name), name);
        }
        await WaitUntilAsync(() => names.All(name => File.Exists(context.BackupFile(name))));
        foreach (string name in names)
        {
            Assert(File.ReadAllText(context.BackupFile(name)) == name, "連続保存した画像の内容が一致しません。");
        }
        context.AssertNoErrors();
    }

    private static async Task AlreadySortedAsync(TestContext context)
    {
        context.Start();
        string name = context.NewName();
        Directory.CreateDirectory(Path.GetDirectoryName(context.SortedFile(name)));
        File.WriteAllText(context.SortedFile(name), "整理済み");
        await WaitUntilAsync(() => File.Exists(context.BackupFile(name)));
        for (int i = 0; i < 5; i++) File.SetLastWriteTimeUtc(context.SortedFile(name), DateTime.UtcNow.AddSeconds(i));
        await Task.Delay(500);
        Assert(File.ReadAllText(context.BackupFile(name)) == "整理済み", "バックアップが変化しました。");
        context.AssertNoErrors();
    }

    private static async Task NestedBackupAsync(TestContext context)
    {
        context.Backup = Path.Combine(context.Source, "バックアップ");
        context.Start();
        string name = context.NewName();
        File.WriteAllText(Path.Combine(context.Source, name), "画像データ");
        await WaitUntilAsync(() => File.Exists(context.SortedFile(name)));
        await Task.Delay(300);
        Assert(!Directory.Exists(context.Backup), "監視先内部にバックアップが作成されました。");
        context.AssertNoErrors();
    }

    private static async Task ShutdownAsync(TestContext context)
    {
        context.Start();
        using var stream = new FileStream(Path.Combine(context.Source, context.NewName()),
            FileMode.Create, FileAccess.Write, FileShare.None);
        await Task.Delay(300);
        var stopwatch = Stopwatch.StartNew();
        context.Monitor.Dispose();
        Assert(stopwatch.Elapsed < TimeSpan.FromSeconds(2), "終了時にロック待ちが継続しました。");
        string name = context.NewName();
        File.WriteAllText(Path.Combine(context.Source, name), "画像データ");
        await Task.Delay(300);
        Assert(!File.Exists(context.SortedFile(name)), "終了後も画像が整理されました。");
        context.AssertNoErrors();
    }

    private static async Task ProcessingErrorAsync(TestContext context)
    {
        context.Start(name => name == "不正.png" ? throw new FormatException("日付が不正です。") : DateFolder);
        File.WriteAllText(Path.Combine(context.Source, "不正.png"), "対象外");
        await WaitUntilAsync(() => !context.Errors.IsEmpty);
        string name = context.NewName();
        File.WriteAllText(Path.Combine(context.Source, name), "画像データ");
        await WaitUntilAsync(() => File.Exists(context.BackupFile(name)));
        Assert(context.Errors.All(error => error is FormatException), "想定外のエラーが発生しました。");
    }

    private static async Task BackupRetryAsync(TestContext context)
    {
        // 同名のファイルでコピー先フォルダーの作成を一時的に妨げます。
        File.WriteAllText(context.Backup, "一時的な障害");
        context.Start();
        string name = context.NewName();
        File.WriteAllText(Path.Combine(context.Source, name), "画像データ");
        await WaitUntilAsync(() => File.Exists(context.SortedFile(name)));
        await Task.Delay(300);
        File.Delete(context.Backup);
        await WaitUntilAsync(() => File.Exists(context.BackupFile(name)));
        Assert(File.ReadAllText(context.BackupFile(name)) == "画像データ", "再試行後のコピー内容が一致しません。");
        context.AssertNoErrors();
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var stopwatch = Stopwatch.StartNew();
        while (!condition())
        {
            if (stopwatch.Elapsed > TimeSpan.FromSeconds(15)) throw new TimeoutException("ファイル処理が完了しませんでした。");
            await Task.Delay(50);
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class TestContext : IDisposable
    {
        private readonly string temporaryRoot = Path.Combine(Path.GetTempPath(), "VRC_SS_Butler.Tests", Guid.NewGuid().ToString("N"));
        public string Source { get; }
        public string Backup { get; set; }
        public ScreenshotMonitor Monitor { get; private set; }
        public ConcurrentQueue<Exception> Errors { get; } = new ConcurrentQueue<Exception>();

        public TestContext()
        {
            Source = Path.Combine(temporaryRoot, "保存先");
            Backup = Path.Combine(temporaryRoot, "コピー先");
            Directory.CreateDirectory(Source);
        }

        public string NewName() => $"VRChat_2026-10-19_12-34-56_{Guid.NewGuid():N}.png";
        public string SortedFile(string name) => Path.Combine(Source, DateFolder, name);
        public string BackupFile(string name) => Path.Combine(Backup, DateFolder, name);

        public void Start(Func<string, string> getFolderName = null)
        {
            Monitor = new ScreenshotMonitor(Source,
                getFolderName ?? (name => name.StartsWith("VRChat_", StringComparison.Ordinal) ? DateFolder : null),
                () => Backup);
            Monitor.Error += (_, e) => Errors.Enqueue(e.GetException());
            Monitor.Start();
        }

        public void AssertNoErrors()
        {
            Assert(Errors.IsEmpty, "監視中にエラーが発生しました: " + string.Join("; ", Errors));
        }

        public void Dispose()
        {
            Monitor?.Dispose();
            Directory.Delete(temporaryRoot, true);
        }
    }
}
