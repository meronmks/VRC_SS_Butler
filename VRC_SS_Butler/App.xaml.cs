using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VRC_SS_Butler
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private NotifyIconWrapper notifyIcon;
        private Mutex mutex = new Mutex(false, "{7C968F36-359B-48F8-A65C-D7C415B8D426}");

        protected override void OnStartup(StartupEventArgs e)
        {
            if(mutex.WaitOne(0, false))
            {
                base.OnStartup(e);
                this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                this.notifyIcon = new NotifyIconWrapper();
            }
            else
            {
                MessageBox.Show("すでに起動しています", "VRC SS Butler", MessageBoxButton.OK, MessageBoxImage.Error);
                mutex.Close();
                mutex = null;
                this.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if(mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
            base.OnExit(e);
            this.notifyIcon.Dispose();
        }
    }
}
