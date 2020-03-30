using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace VRC_SS_Butler
{
    /// <summary>
    /// ProgressWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private string targetCopyPathTextBoxText;
        private FileInfo[] files = null;
        private int compFiles;
        private string progressTextLabelContent;

        public ProgressWindow(string targetCopyPathTextBoxText)
        {
            InitializeComponent();
            this.targetCopyPathTextBoxText = targetCopyPathTextBoxText;
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            var p = new Progress<float>(ShowProgress);
            await Task.Run(() => DoWork(p));
            ssButlerProgressBar.IsIndeterminate = false;
            MessageBox.Show("処理が完了しました", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void ShowProgress(float percent)
        {
            progressTextLabel.Content = progressTextLabelContent;
            if (percent <= 100.0f)
            {
                ssButlerProgressBar.Value = (int)percent;
            }
            else
            {
                ssButlerProgressBar.IsIndeterminate = true;
            }
        }

        private void DoWork(IProgress<float> progress)
        {
            var fileButler = new File_Butler();
            var di = new DirectoryInfo(fileButler.VrcPicFolderPath);
            files = di.GetFiles("*.png", SearchOption.AllDirectories);
            compFiles = 0;
            progressTextLabelContent = $"処理中・・・　{compFiles} / {files.Length}";
            progress.Report(0.0f);
            foreach (var f in files)
            {
                string dateText = fileButler.isRegMatch(f.FullName);
                if (dateText == "") continue;
                var folderName = fileButler.makeFolderName(dateText);
                fileButler.moveFileTo(f.FullName, fileButler.VrcPicFolderPath + folderName);
                compFiles++;
                progressTextLabelContent = $"処理中・・・　{compFiles} / {files.Length}";
                progress.Report((float)compFiles / (float)files.Length);
            }
            progressTextLabelContent = "分類完了！";
            progress.Report(100.0f);
            Thread.Sleep(2000);
            progressTextLabelContent = "空のフォルダをクリーンアップ中・・・";
            progress.Report(200.0f);
            fileButler.emptyFoldersRemove(fileButler.VrcPicFolderPath);
            if (this.targetCopyPathTextBoxText != "")
            {
                progressTextLabelContent = "バックアップ先へコピー中・・・";
                progress.Report(200.0f);
                fileButler.copyFoluderTo(fileButler.VrcPicFolderPath, fileButler.TargetCopyFolderPath);
            }
        }
    }
}
