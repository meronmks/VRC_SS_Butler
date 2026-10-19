using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace VRC_SS_Butler
{
    public partial class NotifyIconWrapper : Component
    {

        private readonly File_Butler fileButler;
        private ScreenshotMonitor screenshotMonitor;
        public NotifyIconWrapper()
        {
            InitializeComponent();

            this.toolStripMenuItem_Open.Click += this.tootStripMenuItem_Open_Click;
            this.toolStripMenuItem_Exit.Click += this.tootStripMenuItem_Exit_Click;

            fileButler = new File_Butler();
            if(fileButler.IsValidPath())
            {
                screenshotMonitor = new ScreenshotMonitor(fileButler.VrcPicFolderPath,
                    name =>
                    {
                        string dateText = fileButler.isRegMatch(name);
                        return dateText == "" ? null : fileButler.makeFolderName(dateText);
                    },
                    () => Properties.Settings.Default.targetPath);
                screenshotMonitor.Error += OnMonitorError;
                screenshotMonitor.Start();
            }
        }

        public NotifyIconWrapper(IContainer container) : this()
        {
            container.Add(this);
        }

        private void tootStripMenuItem_Open_Click(object sender, EventArgs e)
        {
            var wnd = new MainWindow();
            wnd.Show();
        }

        private void tootStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnMonitorError(object sender, ErrorEventArgs e)
        {
            Trace.TraceError(e.GetException().ToString());
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.HasShutdownStarted) return;
            dispatcher.BeginInvoke(new Action(() =>
            {
                if (screenshotMonitor == null) return;
                notifyIcon.ShowBalloonTip(5000, "VRC SS Butler",
                    "エラー: 画像の自動整理に失敗しました。\n" +
                    $"詳細: {e.GetException().Message}\n" +
                    "解決方法: 保存先とアクセス権を確認し、設定から手動整理を実行してください。",
                    System.Windows.Forms.ToolTipIcon.Warning);
            }));
        }

        private void StopMonitoring()
        {
            if (screenshotMonitor == null) return;
            screenshotMonitor.Error -= OnMonitorError;
            screenshotMonitor.Dispose();
            screenshotMonitor = null;
        }

        private void notifyIcon_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                var wnd = new MainWindow();
                wnd.Show();
            }
        }
    }
}
