# VRC_SS_Butler

VRChat のスクリーンショットを日付ごとに整理し、指定したフォルダーへバックアップする Windows アプリです。

## ダウンロード

[GitHub Releases](https://github.com/meronmks/VRC_SS_Butler/releases) で利用するバージョンを開き、**Assets** にある `VRC_SS_Butler-<タグ>-win-x64.zip` をダウンロードしてください。

ZIP をフォルダーに展開し、`VRC_SS_Butler.exe` を起動します。Windows 64bit 用で、.NET ランタイムを同梱しているため、.NET の別途インストールは不要です。`Source code` はソースコードのため、実行する場合は上記の ZIP を選んでください。

同時に添付される `.zip.sha256` は ZIP の SHA-256 チェックサムです。必要に応じて PowerShell の `Get-FileHash <ZIP のパス> -Algorithm SHA256` で確認できます。

## リリースの公開

1. 公開する変更を GitHub にプッシュします。
2. GitHub の **Releases → Draft a new release** で、対象コミットに `v1.6.0` や `v1.6.0-beta.3` の形式のタグを指定します。新しいタグには、このワークフローを含むコミットを選んでください。
3. リリースを公開すると、GitHub Actions の **リリースビルド** が実行されます。プレリリースの公開にも対応しています。
4. ビルドが成功すると、そのリリースの **Assets** に Windows 64bit 用 ZIP とチェックサムが追加されます。公開直後は添付が完了するまで数分かかります。

ビルドではリリースタグのソースを使用し、アプリのバージョンもタグに合わせます。`Release` 構成で .NET を同梱した実行ファイルを生成し、付随する設定ファイルなども含めて ZIP にまとめます。失敗した場合は **Actions → リリースビルド** のログを確認してください。

### 既存リリースの手動ビルド

**Actions → リリースビルド → Run workflow** でブランチに `master` を選び、`tag` に既存リリースのタグ（例: `v1.6.0-beta.2`）を入力します。ワークフローの追加前に公開したタグもビルドできます。リリース自体は事前に作成しておく必要があります。

再実行すると、同じ名前の ZIP とチェックサムを置き換えます。それ以外の添付ファイルは保持されます。追加のシークレット登録は不要で、リポジトリに付属する `GITHUB_TOKEN` を使用します。

## ローカルでのリリースビルド

Windows に .NET 8 SDK をインストールし、リポジトリのルートで実行します。

```powershell
dotnet publish VRC_SS_Butler/VRC_SS_Butler.csproj --configuration Release --runtime win-x64 --self-contained true --output artifacts/publish
```

出力先は `artifacts/publish` です。
