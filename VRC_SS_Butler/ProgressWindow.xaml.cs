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
        public ProgressWindow(string targetCopyPathTextBoxText)
        {
            InitializeComponent();
            this.targetCopyPathTextBoxText = targetCopyPathTextBoxText;
        }

        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            var p = new Progress<float>(ShowProgress);
            await Task.Run(() => DoWork(p));
            MessageBox.Show("処理が完了しました", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void ShowProgress(float percent)
        {
            progressTextLabel.Content = percent + "％完了";
            ssButlerProgressBar.Value = (int)percent;
        }

        private void DoWork(IProgress<float> progress)
        {
            var fileButler = new File_Butler();
            var di = new DirectoryInfo(fileButler.VrcPicFolderPath);
            var files = di.GetFiles("*.png", SearchOption.AllDirectories);
            var compFiles = 0;
            progress.Report(0.0f);
            foreach (var f in files)
            {
                string dateText = fileButler.isRegMatch(f.FullName);
                if (dateText == "") continue;
                var folderName = fileButler.makeFolderName(dateText);
                fileButler.moveFileTo(f.FullName, fileButler.VrcPicFolderPath + folderName);
                compFiles++;
                progress.Report((float)compFiles / (float)files.Length);
            }
            progress.Report(100.0f);
            if (this.targetCopyPathTextBoxText != "")
            {
                fileButler.copyFoluderTo(fileButler.VrcPicFolderPath, fileButler.TargetCopyFolderPath);
            }
            Thread.Sleep(2000);
            fileButler.emptyFoldersRemove(fileButler.VrcPicFolderPath);
        }
    }
}
