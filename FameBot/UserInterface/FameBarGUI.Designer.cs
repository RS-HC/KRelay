namespace FameBot.UserInterface
{
    partial class FameBarGUI
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FameBarGUI));
            this.onTopBox = new System.Windows.Forms.CheckBox();
            this.fameBar = new System.Windows.Forms.ProgressBar();
            this.fameText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // onTopBox
            // 
            this.onTopBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.onTopBox.AutoSize = true;
            this.onTopBox.Checked = true;
            this.onTopBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.onTopBox.Location = new System.Drawing.Point(9, 39);
            this.onTopBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.onTopBox.Name = "onTopBox";
            this.onTopBox.Size = new System.Drawing.Size(133, 17);
            this.onTopBox.TabIndex = 0;
            this.onTopBox.Text = "Window always on top";
            this.onTopBox.UseVisualStyleBackColor = true;
            this.onTopBox.CheckedChanged += new System.EventHandler(this.onTopBox_CheckedChanged);
            // 
            // fameBar
            // 
            this.fameBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fameBar.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.fameBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
            this.fameBar.Location = new System.Drawing.Point(9, 8);
            this.fameBar.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.fameBar.Name = "fameBar";
            this.fameBar.Size = new System.Drawing.Size(235, 28);
            this.fameBar.TabIndex = 1;
            // 
            // fameText
            // 
            this.fameText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fameText.Location = new System.Drawing.Point(141, 43);
            this.fameText.Name = "fameText";
            this.fameText.Size = new System.Drawing.Size(100, 15);
            this.fameText.TabIndex = 2;
            this.fameText.Text = "0 / 0";
            this.fameText.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FameBarGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(252, 69);
            this.Controls.Add(this.fameText);
            this.Controls.Add(this.fameBar);
            this.Controls.Add(this.onTopBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MinimumSize = new System.Drawing.Size(173, 102);
            this.Name = "FameBarGUI";
            this.Text = "[FameBot] Fame";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox onTopBox;
        private System.Windows.Forms.ProgressBar fameBar;
        private System.Windows.Forms.Label fameText;
    }
}