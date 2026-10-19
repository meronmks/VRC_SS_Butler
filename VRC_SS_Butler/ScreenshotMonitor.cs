using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace VRC_SS_Butler
{
    /// <summary>
    /// 保存通知の受信と、書き込み完了後の画像の整理を別々に実行します。
    /// </summary>
    internal sealed class ScreenshotMonitor : IDisposable
    {
        private readonly string rootPath;
        private readonly Func<string, string> getFolderName;
        private readonly Func<string> getBackupPath;
        private readonly FileSystemWatcher watcher;
        private readonly CancellationTokenSource cancellation = new CancellationTokenSource();
        private readonly Channel<string> queue = Channel.CreateUnbounded<string>(
            new UnboundedChannelOptions { SingleReader = true });
        private readonly ConcurrentDictionary<string, byte> pending =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
        private readonly Task worker;
        private int disposed;

        public event EventHandler<ErrorEventArgs> Error;

        public ScreenshotMonitor(string rootPath, Func<string, string> getFolderName,
            Func<string> getBackupPath)
        {
            this.rootPath = Path.GetFullPath(rootPath);
            this.getFolderName = getFolderName;
            this.getBackupPath = getBackupPath;
            watcher = new FileSystemWatcher(this.rootPath, "*.png")
            {
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size
            };
            watcher.Created += OnFileChanged;
            watcher.Changed += OnFileChanged;
            watcher.Renamed += OnFileChanged;
            watcher.Error += OnWatcherError;
            worker = Task.Run(ProcessQueueAsync);
        }

        public void Start()
        {
            // パスと購読を設定してから監視を開始します。
            watcher.EnableRaisingEvents = true;
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            Enqueue(e.FullPath);
        }

        private void Enqueue(string path)
        {
            if (Volatile.Read(ref disposed) == 0 && pending.TryAdd(path, 0))
            {
                if (!queue.Writer.TryWrite(path))
                {
                    pending.TryRemove(path, out _);
                }
            }
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Error?.Invoke(this, e);
            if (e.GetException() is InternalBufferOverflowException)
            {
                // 通知を取りこぼした場合は、ワーカー側で保存先を再確認します。
                Enqueue(rootPath);
            }
        }

        private async Task ProcessQueueAsync()
        {
            try
            {
                await foreach (string path in queue.Reader.ReadAllAsync(cancellation.Token))
                {
                    try
                    {
                        if (path == rootPath)
                        {
                            foreach (string file in Directory.EnumerateFiles(rootPath, "*.png",
                                SearchOption.AllDirectories))
                            {
                                Enqueue(file);
                            }
                        }
                        else
                        {
                            await ProcessFileAsync(path, cancellation.Token);
                        }
                    }
                    catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        // 画像単位の失敗で常駐処理全体を終了させません。
                        Error?.Invoke(this, new ErrorEventArgs(ex));
                    }
                    finally
                    {
                        pending.TryRemove(path, out _);
                    }
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
                // 終了時は書き込み完了待ちも中断します。
            }
        }

        private async Task ProcessFileAsync(string path, CancellationToken token)
        {
            string folderName = getFolderName(Path.GetFileName(path));
            if (string.IsNullOrEmpty(folderName)) return;

            string destinationFolder = Path.Combine(rootPath, folderName);
            string destination = Path.Combine(destinationFolder, Path.GetFileName(path));
            string backupRoot = getBackupPath();
            string backup = null;
            if (!string.IsNullOrWhiteSpace(backupRoot))
            {
                backupRoot = Path.GetFullPath(backupRoot);
                // 監視先の内側へのバックアップによる再処理を防ぎます。
                string relative = Path.GetRelativePath(rootPath, backupRoot);
                if (Path.IsPathRooted(relative) || relative == ".." ||
                    relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                {
                    backup = Path.Combine(backupRoot, folderName, Path.GetFileName(path));
                }
            }

            for (int attempt = 0; ; attempt++)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    // 書き込み中のファイルは開けない共有モードを使い、移動・コピー中も保持します。
                    using (var source = new FileStream(path, FileMode.Open, FileAccess.Read,
                        FileShare.Read | FileShare.Delete))
                    {
                        if (!string.Equals(path, destination, StringComparison.OrdinalIgnoreCase))
                        {
                            Directory.CreateDirectory(destinationFolder);
                            File.Move(path, destination);
                            // コピーが失敗しても、移動済みのファイルから再試行できます。
                            path = destination;
                        }

                        if (backup != null && !File.Exists(backup))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(backup));
                            string temporaryBackup = backup + "." + Guid.NewGuid().ToString("N") + ".tmp";
                            try
                            {
                                // 途中でコピーに失敗しても、未完成の画像をバックアップ済みと判定させません。
                                File.Copy(path, temporaryBackup);
                                File.Move(temporaryBackup, backup);
                            }
                            finally
                            {
                                if (File.Exists(temporaryBackup)) File.Delete(temporaryBackup);
                            }
                        }
                    }
                    return;
                }
                catch (FileNotFoundException)
                {
                    // 重複通知や手動整理により移動済みの場合は処理不要です。
                    return;
                }
                catch (DirectoryNotFoundException) when (!Directory.Exists(Path.GetDirectoryName(path)))
                {
                    return;
                }
                catch (IOException) when (attempt < 119)
                {
                    // 作成通知は保存完了前にも届くため、最大約30秒間再試行します。
                    await Task.Delay(250, token);
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref disposed, 1) != 0) return;
            watcher.Dispose();
            queue.Writer.TryComplete();
            cancellation.Cancel();
            worker.GetAwaiter().GetResult();
            cancellation.Dispose();
        }
    }
}
