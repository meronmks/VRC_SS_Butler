using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VRC_SS_Butler
{
    public partial class NotifyIconWrapper : Component
    {
        public NotifyIconWrapper()
        {
            InitializeComponent();

            this.toolStripMenuItem_Open.Click += this.tootStripMenuItem_Open_Click;
            this.toolStripMenuItem_Exit.Click += this.tootStripMenuItem_Exit_Click;

            fileSystemWatcher.Path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\VRChat";
            fileSystemWatcher.Created += file_Created;
        }

        public NotifyIconWrapper(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        private void tootStripMenuItem_Open_Click(object sender, EventArgs e)
        {
            var wnd = new MainWindow();
            wnd.Show();
        }

        private void tootStripMenuItem_Exit_Click(object sender, EventArgs e)
        {
            fileSystemWatcher.Dispose();
            Application.Current.Shutdown();
        }

        private void file_Created(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(3000);
            var fileButler = new File_Butler();
            string dateText = fileButler.isRegMatch(e.Name);
            var folderName = fileButler.makeFolderName(dateText);
            fileButler.moveFileTo(e.FullPath, Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\VRChat\" + folderName);

            if (Properties.Settings.Default.targetPath != "")
            {
                fileButler.copyFoluderTo(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\VRChat\" + folderName, Properties.Settings.Default.targetPath + @"\VRChat\" + folderName);
            }
        }
    }
}
