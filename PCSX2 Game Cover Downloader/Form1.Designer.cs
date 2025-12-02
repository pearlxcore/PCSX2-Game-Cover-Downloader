namespace PCSX2_Game_Cover_Downloader
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            btnBrowseCache = new Button();
            tbGameListLoc = new TextBox();
            tbGameCoverDir = new TextBox();
            btnDownload = new Button();
            label1 = new Label();
            label2 = new Label();
            btnBrowseCover = new Button();
            progressBar1 = new ProgressBar();
            lblStatus = new Label();
            cbOverwrite = new CheckBox();
            lbLog = new ListBox();
            lblVersion = new Label();
            SuspendLayout();
            // 
            // btnBrowseCache
            // 
            btnBrowseCache.Font = new Font("Segoe UI Variable Small", 12F);
            btnBrowseCache.Location = new Point(366, 53);
            btnBrowseCache.Name = "btnBrowseCache";
            btnBrowseCache.Size = new Size(75, 36);
            btnBrowseCache.TabIndex = 0;
            btnBrowseCache.Text = "browse";
            btnBrowseCache.UseVisualStyleBackColor = true;
            btnBrowseCache.Click += btnGameListLocation_Click;
            // 
            // tbGameListLoc
            // 
            tbGameListLoc.AllowDrop = true;
            tbGameListLoc.Font = new Font("Segoe UI Variable Small", 12F);
            tbGameListLoc.Location = new Point(47, 57);
            tbGameListLoc.Name = "tbGameListLoc";
            tbGameListLoc.Size = new Size(301, 29);
            tbGameListLoc.TabIndex = 1;
            tbGameListLoc.DragDrop += tbGameListLoc_DragDrop;
            // 
            // tbGameCoverDir
            // 
            tbGameCoverDir.AllowDrop = true;
            tbGameCoverDir.Font = new Font("Segoe UI Variable Small", 12F);
            tbGameCoverDir.Location = new Point(47, 126);
            tbGameCoverDir.Name = "tbGameCoverDir";
            tbGameCoverDir.Size = new Size(301, 29);
            tbGameCoverDir.TabIndex = 3;
            tbGameCoverDir.DragDrop += tbGameCoverDir_DragDrop;
            // 
            // btnDownload
            // 
            btnDownload.Font = new Font("Segoe UI Variable Small", 12F);
            btnDownload.Location = new Point(47, 194);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(394, 32);
            btnDownload.TabIndex = 4;
            btnDownload.Text = "Download";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Variable Small", 12F);
            label1.Location = new Point(47, 33);
            label1.Name = "label1";
            label1.Size = new Size(153, 21);
            label1.TabIndex = 5;
            label1.Text = "Game list cache file:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Variable Small", 12F);
            label2.Location = new Point(47, 99);
            label2.Name = "label2";
            label2.Size = new Size(172, 21);
            label2.TabIndex = 6;
            label2.Text = "Game cover directory:";
            // 
            // btnBrowseCover
            // 
            btnBrowseCover.Font = new Font("Segoe UI Variable Small", 12F);
            btnBrowseCover.Location = new Point(366, 122);
            btnBrowseCover.Name = "btnBrowseCover";
            btnBrowseCover.Size = new Size(75, 36);
            btnBrowseCover.TabIndex = 7;
            btnBrowseCover.Text = "browse";
            btnBrowseCover.UseVisualStyleBackColor = true;
            btnBrowseCover.Click += btnGameCoverDir_Click;
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(47, 260);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(394, 10);
            progressBar1.TabIndex = 8;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Font = new Font("Segoe UI Variable Small", 12F);
            lblStatus.Location = new Point(47, 236);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(55, 21);
            lblStatus.TabIndex = 10;
            lblStatus.Text = "Ready";
            // 
            // cbOverwrite
            // 
            cbOverwrite.AutoSize = true;
            cbOverwrite.Font = new Font("Segoe UI Variable Small", 12F);
            cbOverwrite.Location = new Point(47, 163);
            cbOverwrite.Name = "cbOverwrite";
            cbOverwrite.Size = new Size(191, 25);
            cbOverwrite.TabIndex = 11;
            cbOverwrite.Text = "Overwrite game cover";
            cbOverwrite.UseVisualStyleBackColor = true;
            // 
            // lbLog
            // 
            lbLog.FormattingEnabled = true;
            lbLog.Location = new Point(47, 276);
            lbLog.Name = "lbLog";
            lbLog.Size = new Size(394, 199);
            lbLog.TabIndex = 12;
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new Font("Segoe UI Variable Small", 12F);
            lblVersion.Location = new Point(47, 482);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new Size(50, 21);
            lblVersion.TabIndex = 13;
            lblVersion.Text = "v1.0.0";
            // 
            // Form1
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(489, 537);
            Controls.Add(lblVersion);
            Controls.Add(lbLog);
            Controls.Add(cbOverwrite);
            Controls.Add(lblStatus);
            Controls.Add(progressBar1);
            Controls.Add(btnBrowseCover);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnDownload);
            Controls.Add(tbGameCoverDir);
            Controls.Add(tbGameListLoc);
            Controls.Add(btnBrowseCache);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "PCSX2 Game Cover Downloader by pearlxcore";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnBrowseCache;
        private TextBox tbGameListLoc;
        private TextBox tbGameCoverDir;
        private Button btnDownload;
        private Label label1;
        private Label label2;
        private Button btnBrowseCover;
        private ProgressBar progressBar1;
        private Label lblStatus;
        private CheckBox cbOverwrite;
        private ListBox lbLog;
        private Label lblVersion;
    }
}
