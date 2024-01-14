namespace VRC_SS_Butler
{
    partial class NotifyIconWrapper
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NotifyIconWrapper));
            notifyIcon = new System.Windows.Forms.NotifyIcon(components);
            contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(components);
            toolStripMenuItem_Open = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem_Exit = new System.Windows.Forms.ToolStripMenuItem();
            fileSystemWatcher = new System.IO.FileSystemWatcher();
            contextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)fileSystemWatcher).BeginInit();
            // 
            // notifyIcon
            // 
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.Icon = (System.Drawing.Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "VRC_SS_Butler";
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            // 
            // contextMenuStrip
            // 
            contextMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { toolStripMenuItem_Open, toolStripMenuItem_Exit });
            contextMenuStrip.Name = "contextMenuStrip";
            contextMenuStrip.Size = new System.Drawing.Size(99, 48);
            // 
            // toolStripMenuItem_Open
            // 
            toolStripMenuItem_Open.Name = "toolStripMenuItem_Open";
            toolStripMenuItem_Open.Size = new System.Drawing.Size(98, 22);
            toolStripMenuItem_Open.Text = "設定";
            // 
            // toolStripMenuItem_Exit
            // 
            toolStripMenuItem_Exit.Name = "toolStripMenuItem_Exit";
            toolStripMenuItem_Exit.Size = new System.Drawing.Size(98, 22);
            toolStripMenuItem_Exit.Text = "終了";
            // 
            // fileSystemWatcher
            // 
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.Filter = "*.png";
            fileSystemWatcher.SynchronizingObject = null;
            contextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)fileSystemWatcher).EndInit();
        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Open;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_Exit;
        private System.IO.FileSystemWatcher fileSystemWatcher;
    }
}
