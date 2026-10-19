# ファイル監視の回帰テスト

Windows上の.NET 8以降で、リポジトリ直下から実行します。

```powershell
dotnet run --project VRC_SS_Butler.Tests
```

実際の `FileSystemWatcher` と一時フォルダーを使って、自動整理・バックアップ、サブフォルダー、書き込みロック、名前変更、重複通知、連続保存、エラー後の継続、終了時のキャンセルを検証します。実際のVRChatの画像や設定は変更しません。追加のテストパッケージは不要で、失敗時は終了コード1を返します。

アプリと同じ `ScreenshotMonitor.cs` をリンクして実行します。トレイアイコンとVRChat本体を含む画面操作はこのテストの対象外です。
