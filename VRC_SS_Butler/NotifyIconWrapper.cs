﻿using System;
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

        private File_Butler fileButler;
        public NotifyIconWrapper()
        {
            InitializeComponent();

            this.toolStripMenuItem_Open.Click += this.tootStripMenuItem_Open_Click;
            this.toolStripMenuItem_Exit.Click += this.tootStripMenuItem_Exit_Click;

            fileButler = new File_Butler();
            if(fileButler.IsValidPath())
            {
                fileSystemWatcher.Path = fileButler.VrcPicFolderPath;
                fileSystemWatcher.Created += file_Created;
            }
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
            fileSystemWatcher.Created -= file_Created;
            fileSystemWatcher.Dispose();
            Application.Current.Shutdown();
        }

        private void file_Created(object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(2000);

            string dateText = fileButler.isRegMatch(e.Name);
            var folderName = fileButler.makeFolderName(dateText);
            fileButler.moveFileTo(e.FullPath, fileButler.VrcPicFolderPath + folderName);

            if (Properties.Settings.Default.targetPath != "")
            {
                fileButler.copyFoluderTo(fileButler.VrcPicFolderPath + folderName, fileButler.TargetCopyFolderPath + folderName);
            }
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
