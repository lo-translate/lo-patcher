namespace LoPatcher
{
    partial class MainForm
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
            this.dialogChooseInput = new System.Windows.Forms.OpenFileDialog();
            this.groupBoxBundleFile = new System.Windows.Forms.GroupBox();
            this.labelSelectedFile = new System.Windows.Forms.Label();
            this.buttonChooseBundle = new System.Windows.Forms.Button();
            this.buttonPatch = new System.Windows.Forms.Button();
            this.groupBoxLanguageData = new System.Windows.Forms.GroupBox();
            this.buttonLanguageUpdate = new System.Windows.Forms.Button();
            this.labelLanguageVersion = new System.Windows.Forms.Label();
            this.dialogChooseOutput = new System.Windows.Forms.SaveFileDialog();
            this.labelStatus = new System.Windows.Forms.Label();
            this.groupBoxBundleFile.SuspendLayout();
            this.groupBoxLanguageData.SuspendLayout();
            this.SuspendLayout();
            // 
            // dialogChooseInput
            // 
            this.dialogChooseInput.FileName = "__data";
            this.dialogChooseInput.Filter = "__data files|__data*|All files|*.*";
            // 
            // groupBoxBundleFile
            // 
            this.groupBoxBundleFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBundleFile.Controls.Add(this.labelSelectedFile);
            this.groupBoxBundleFile.Controls.Add(this.buttonChooseBundle);
            this.groupBoxBundleFile.Location = new System.Drawing.Point(13, 13);
            this.groupBoxBundleFile.Name = "groupBoxBundleFile";
            this.groupBoxBundleFile.Size = new System.Drawing.Size(379, 57);
            this.groupBoxBundleFile.TabIndex = 0;
            this.groupBoxBundleFile.TabStop = false;
            this.groupBoxBundleFile.Text = "Bundle file";
            // 
            // labelSelectedFile
            // 
            this.labelSelectedFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSelectedFile.AutoEllipsis = true;
            this.labelSelectedFile.Location = new System.Drawing.Point(93, 28);
            this.labelSelectedFile.Name = "labelSelectedFile";
            this.labelSelectedFile.Size = new System.Drawing.Size(275, 20);
            this.labelSelectedFile.TabIndex = 1;
            this.labelSelectedFile.Text = "No file selected";
            // 
            // buttonChooseBundle
            // 
            this.buttonChooseBundle.Location = new System.Drawing.Point(11, 22);
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
            this.buttonPatch.Location = new System.Drawing.Point(290, 150);
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
            this.groupBoxLanguageData.Controls.Add(this.buttonLanguageUpdate);
            this.groupBoxLanguageData.Controls.Add(this.labelLanguageVersion);
            this.groupBoxLanguageData.Enabled = false;
            this.groupBoxLanguageData.Location = new System.Drawing.Point(13, 81);
            this.groupBoxLanguageData.Name = "groupBoxLanguageData";
            this.groupBoxLanguageData.Size = new System.Drawing.Size(379, 57);
            this.groupBoxLanguageData.TabIndex = 2;
            this.groupBoxLanguageData.TabStop = false;
            this.groupBoxLanguageData.Text = "Language data";
            // 
            // buttonLanguageUpdate
            // 
            this.buttonLanguageUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonLanguageUpdate.Location = new System.Drawing.Point(249, 20);
            this.buttonLanguageUpdate.Name = "buttonLanguageUpdate";
            this.buttonLanguageUpdate.Size = new System.Drawing.Size(119, 25);
            this.buttonLanguageUpdate.TabIndex = 1;
            this.buttonLanguageUpdate.Text = "Check for update";
            this.buttonLanguageUpdate.UseVisualStyleBackColor = true;
            // 
            // labelLanguageVersion
            // 
            this.labelLanguageVersion.AutoSize = true;
            this.labelLanguageVersion.Location = new System.Drawing.Point(11, 25);
            this.labelLanguageVersion.Name = "labelLanguageVersion";
            this.labelLanguageVersion.Size = new System.Drawing.Size(139, 15);
            this.labelLanguageVersion.TabIndex = 0;
            this.labelLanguageVersion.Text = "Last updated: 2020-04-05";
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelStatus.AutoEllipsis = true;
            this.labelStatus.Location = new System.Drawing.Point(13, 146);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(271, 32);
            this.labelStatus.TabIndex = 3;
            this.labelStatus.Text = "Status text";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(404, 186);
            this.Controls.Add(this.labelStatus);
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
        private System.Windows.Forms.Label labelLanguageVersion;
        private System.Windows.Forms.SaveFileDialog dialogChooseOutput;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.GroupBox groupBoxBundleFile;
    }
}

