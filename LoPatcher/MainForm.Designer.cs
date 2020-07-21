namespace LoPatcher
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dialogChooseInput = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxBundleFile = new System.Windows.Forms.GroupBox();
            this.labelSelectedFile = new System.Windows.Forms.Label();
            this.buttonChooseBundle = new System.Windows.Forms.Button();
            this.buttonPatch = new System.Windows.Forms.Button();
            this.groupBoxLanguageData = new System.Windows.Forms.GroupBox();
            this.linkLabelCheckLanguageUpdate = new System.Windows.Forms.LinkLabel();
            this.labelNewestLangVersion = new System.Windows.Forms.Label();
            this.labelNewestLangVersionLabel = new System.Windows.Forms.Label();
            this.labelCurrentLangVersion = new System.Windows.Forms.Label();
            this.buttonLanguageUpdate = new System.Windows.Forms.Button();
            this.labelCurrentLangVersionLabel = new System.Windows.Forms.Label();
            this.dialogChoosePatchOutput = new System.Windows.Forms.SaveFileDialog();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelCurrentStatus = new System.Windows.Forms.Label();
            this.groupBoxBundleFile.SuspendLayout();
            this.groupBoxLanguageData.SuspendLayout();
            this.SuspendLayout();
            // 
            // dialogChooseInput
            // 
            this.dialogChooseInput.Filter = "UABE extracted asset files (raw)|*.dat|All files|*.*";
            this.dialogChooseInput.Multiselect = true;
            // 
            // groupBoxBundleFile
            // 
            this.groupBoxBundleFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBundleFile.Controls.Add(this.labelSelectedFile);
            this.groupBoxBundleFile.Controls.Add(this.buttonChooseBundle);
            this.groupBoxBundleFile.Location = new System.Drawing.Point(13, 13);
            this.groupBoxBundleFile.Name = "groupBoxBundleFile";
            this.groupBoxBundleFile.Size = new System.Drawing.Size(459, 67);
            this.groupBoxBundleFile.TabIndex = 0;
            this.groupBoxBundleFile.TabStop = false;
            this.groupBoxBundleFile.Text = "Asset files to patch";
            // 
            // labelSelectedFile
            // 
            this.labelSelectedFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSelectedFile.AutoEllipsis = true;
            this.labelSelectedFile.Location = new System.Drawing.Point(91, 20);
            this.labelSelectedFile.Name = "labelSelectedFile";
            this.labelSelectedFile.Size = new System.Drawing.Size(357, 38);
            this.labelSelectedFile.TabIndex = 1;
            this.labelSelectedFile.Text = "No file(s) selected";
            this.labelSelectedFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelSelectedFile.Click += new System.EventHandler(this.LabelSelectedFile_Click);
            // 
            // buttonChooseBundle
            // 
            this.buttonChooseBundle.Location = new System.Drawing.Point(11, 27);
            this.buttonChooseBundle.Name = "buttonChooseBundle";
            this.buttonChooseBundle.Size = new System.Drawing.Size(75, 25);
            this.buttonChooseBundle.TabIndex = 0;
            this.buttonChooseBundle.Text = "Browse...";
            this.buttonChooseBundle.UseVisualStyleBackColor = true;
            this.buttonChooseBundle.Click += new System.EventHandler(this.ButtonChooseBundle_Click);
            // 
            // buttonPatch
            // 
            this.buttonPatch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonPatch.Enabled = false;
            this.buttonPatch.Location = new System.Drawing.Point(370, 206);
            this.buttonPatch.Name = "buttonPatch";
            this.buttonPatch.Size = new System.Drawing.Size(102, 25);
            this.buttonPatch.TabIndex = 1;
            this.buttonPatch.Text = "Start Patching";
            this.buttonPatch.UseVisualStyleBackColor = true;
            this.buttonPatch.Click += new System.EventHandler(this.ButtonPatch_Click);
            // 
            // groupBoxLanguageData
            // 
            this.groupBoxLanguageData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLanguageData.Controls.Add(this.linkLabelCheckLanguageUpdate);
            this.groupBoxLanguageData.Controls.Add(this.labelNewestLangVersion);
            this.groupBoxLanguageData.Controls.Add(this.labelNewestLangVersionLabel);
            this.groupBoxLanguageData.Controls.Add(this.labelCurrentLangVersion);
            this.groupBoxLanguageData.Controls.Add(this.buttonLanguageUpdate);
            this.groupBoxLanguageData.Controls.Add(this.labelCurrentLangVersionLabel);
            this.groupBoxLanguageData.Location = new System.Drawing.Point(13, 91);
            this.groupBoxLanguageData.Name = "groupBoxLanguageData";
            this.groupBoxLanguageData.Size = new System.Drawing.Size(459, 77);
            this.groupBoxLanguageData.TabIndex = 2;
            this.groupBoxLanguageData.TabStop = false;
            this.groupBoxLanguageData.Text = "Language data";
            // 
            // linkLabelCheckLanguageUpdate
            // 
            this.linkLabelCheckLanguageUpdate.AutoSize = true;
            this.linkLabelCheckLanguageUpdate.Location = new System.Drawing.Point(109, 47);
            this.linkLabelCheckLanguageUpdate.Name = "linkLabelCheckLanguageUpdate";
            this.linkLabelCheckLanguageUpdate.Size = new System.Drawing.Size(40, 15);
            this.linkLabelCheckLanguageUpdate.TabIndex = 5;
            this.linkLabelCheckLanguageUpdate.TabStop = true;
            this.linkLabelCheckLanguageUpdate.Text = "Check";
            this.linkLabelCheckLanguageUpdate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabelCheckLanguageUpdate_LinkClicked);
            // 
            // labelNewestLangVersion
            // 
            this.labelNewestLangVersion.AutoEllipsis = true;
            this.labelNewestLangVersion.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.labelNewestLangVersion.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelNewestLangVersion.Location = new System.Drawing.Point(108, 47);
            this.labelNewestLangVersion.Name = "labelNewestLangVersion";
            this.labelNewestLangVersion.Size = new System.Drawing.Size(210, 15);
            this.labelNewestLangVersion.TabIndex = 4;
            this.labelNewestLangVersion.Text = "0000.00.00.00";
            // 
            // labelNewestLangVersionLabel
            // 
            this.labelNewestLangVersionLabel.AutoSize = true;
            this.labelNewestLangVersionLabel.Location = new System.Drawing.Point(12, 47);
            this.labelNewestLangVersionLabel.Name = "labelNewestLangVersionLabel";
            this.labelNewestLangVersionLabel.Size = new System.Drawing.Size(90, 15);
            this.labelNewestLangVersionLabel.TabIndex = 3;
            this.labelNewestLangVersionLabel.Text = "Newest version:";
            // 
            // labelCurrentLangVersion
            // 
            this.labelCurrentLangVersion.AutoEllipsis = true;
            this.labelCurrentLangVersion.Location = new System.Drawing.Point(108, 27);
            this.labelCurrentLangVersion.Name = "labelCurrentLangVersion";
            this.labelCurrentLangVersion.Size = new System.Drawing.Size(210, 15);
            this.labelCurrentLangVersion.TabIndex = 2;
            this.labelCurrentLangVersion.Text = "0000.00.00.00";
            // 
            // buttonLanguageUpdate
            // 
            this.buttonLanguageUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLanguageUpdate.Location = new System.Drawing.Point(330, 32);
            this.buttonLanguageUpdate.Name = "buttonLanguageUpdate";
            this.buttonLanguageUpdate.Size = new System.Drawing.Size(118, 25);
            this.buttonLanguageUpdate.TabIndex = 1;
            this.buttonLanguageUpdate.Text = "Download update";
            this.buttonLanguageUpdate.UseVisualStyleBackColor = true;
            this.buttonLanguageUpdate.Click += new System.EventHandler(this.ButtonLanguageUpdate_Click);
            // 
            // labelCurrentLangVersionLabel
            // 
            this.labelCurrentLangVersionLabel.AutoSize = true;
            this.labelCurrentLangVersionLabel.Location = new System.Drawing.Point(11, 27);
            this.labelCurrentLangVersionLabel.Name = "labelCurrentLangVersionLabel";
            this.labelCurrentLangVersionLabel.Size = new System.Drawing.Size(91, 15);
            this.labelCurrentLangVersionLabel.TabIndex = 0;
            this.labelCurrentLangVersionLabel.Text = "Current version:";
            // 
            // dialogChoosePatchOutput
            // 
            this.dialogChoosePatchOutput.FileName = "__data.modified";
            this.dialogChoosePatchOutput.Filter = "__data files|__data*|All files|*.*";
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 207);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(344, 24);
            this.progressBar.TabIndex = 3;
            this.progressBar.Visible = false;
            // 
            // labelCurrentStatus
            // 
            this.labelCurrentStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCurrentStatus.AutoEllipsis = true;
            this.labelCurrentStatus.Location = new System.Drawing.Point(13, 180);
            this.labelCurrentStatus.Name = "labelCurrentStatus";
            this.labelCurrentStatus.Size = new System.Drawing.Size(459, 19);
            this.labelCurrentStatus.TabIndex = 4;
            this.labelCurrentStatus.Text = "Processing...";
            this.labelCurrentStatus.Visible = false;
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 242);
            this.Controls.Add(this.labelCurrentStatus);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.groupBoxLanguageData);
            this.Controls.Add(this.buttonPatch);
            this.Controls.Add(this.groupBoxBundleFile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "Lo Patcher";
            this.groupBoxBundleFile.ResumeLayout(false);
            this.groupBoxLanguageData.ResumeLayout(false);
            this.groupBoxLanguageData.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.OpenFileDialog dialogChooseInput;
        private System.Windows.Forms.Label labelSelectedFile;
        private System.Windows.Forms.Button buttonChooseBundle;
        private System.Windows.Forms.Button buttonPatch;
        private System.Windows.Forms.GroupBox groupBoxLanguageData;
        private System.Windows.Forms.Button buttonLanguageUpdate;
        private System.Windows.Forms.Label labelCurrentLangVersionLabel;
        private System.Windows.Forms.SaveFileDialog dialogChoosePatchOutput;
        private System.Windows.Forms.GroupBox groupBoxBundleFile;
        private System.Windows.Forms.Label labelCurrentLangVersion;
        private System.Windows.Forms.Label labelNewestLangVersionLabel;
        private System.Windows.Forms.Label labelNewestLangVersion;
        private System.Windows.Forms.LinkLabel linkLabelCheckLanguageUpdate;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelCurrentStatus;
    }
}

