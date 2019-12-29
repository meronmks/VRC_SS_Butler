using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VRC_SS_Butler
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            targetCopyPathTextBox.Text = Properties.Settings.Default.targetPath;
            if (Properties.Settings.Default.folderMakeDays)
            {
                folderMakeOptionDays.IsChecked = true;
                folderMakeOptionYear.IsChecked = false;
            }
            else
            {
                folderMakeOptionDays.IsChecked = false;
                folderMakeOptionYear.IsChecked = true;
            }
            timeTextBox.Text = Properties.Settings.Default.timeChangeLine.ToString();
        }

        private void onClick_okButton(object sender, RoutedEventArgs e)
        {
            settingsSave();
            this.Close();
        }

        private void onClick_folderSyncButton(object sender, RoutedEventArgs e)
        {
            var mesBoxResult = MessageBox.Show("処理を実行しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (mesBoxResult == MessageBoxResult.No) return;
            settingsSave();
            var progressWindow = new ProgressWindow(targetCopyPathTextBox.Text);
            progressWindow.ShowDialog();
        }

        private void settingsSave()
        {
            Properties.Settings.Default.targetPath = targetCopyPathTextBox.Text;
            Properties.Settings.Default.folderMakeDays = folderMakeOptionDays.IsChecked.Value;
            Properties.Settings.Default.timeChangeLine = int.Parse(timeTextBox.Text);
            Properties.Settings.Default.Save();
        }

        private void enableAutoStart_Click(object sender, RoutedEventArgs e)
        {
            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            regKey.SetValue(Assembly.GetExecutingAssembly().GetName().Name, Environment.GetCommandLineArgs()[0]);
            MessageBox.Show("スタートアップ登録しました", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void desableAutoStart_Click(object sender, RoutedEventArgs e)
        {
            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            regKey.DeleteValue(Assembly.GetExecutingAssembly().GetName().Name, false);
            MessageBox.Show("スタートアップ解除しました", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void timeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 0-9のみ
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }
        private void timeTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // 貼り付けを許可しない
            if (e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}
