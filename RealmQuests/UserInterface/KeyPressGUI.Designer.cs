﻿namespace FameBot.UserInterface
{
    partial class KeyPressGUI
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
            this.wBox = new System.Windows.Forms.PictureBox();
            this.aBox = new System.Windows.Forms.PictureBox();
            this.sBox = new System.Windows.Forms.PictureBox();
            this.dBox = new System.Windows.Forms.PictureBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.wBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.aBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dBox)).BeginInit();
            this.SuspendLayout();
            // 
            // wBox
            // 
            this.wBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.wBox.Image = global::FameBot.Properties.Resources.w_off;
            this.wBox.InitialImage = null;
            this.wBox.Location = new System.Drawing.Point(82, 12);
            this.wBox.Name = "wBox";
            this.wBox.Size = new System.Drawing.Size(64, 64);
            this.wBox.TabIndex = 0;
            this.wBox.TabStop = false;
            // 
            // aBox
            // 
            this.aBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.aBox.Image = global::FameBot.Properties.Resources.a_off;
            this.aBox.InitialImage = null;
            this.aBox.Location = new System.Drawing.Point(11, 82);
            this.aBox.Name = "aBox";
            this.aBox.Size = new System.Drawing.Size(64, 64);
            this.aBox.TabIndex = 1;
            this.aBox.TabStop = false;
            // 
            // sBox
            // 
            this.sBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.sBox.Image = global::FameBot.Properties.Resources.s_off;
            this.sBox.InitialImage = null;
            this.sBox.Location = new System.Drawing.Point(82, 82);
            this.sBox.Name = "sBox";
            this.sBox.Size = new System.Drawing.Size(64, 64);
            this.sBox.TabIndex = 2;
            this.sBox.TabStop = false;
            // 
            // dBox
            // 
            this.dBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.dBox.Image = global::FameBot.Properties.Resources.d_off;
            this.dBox.InitialImage = null;
            this.dBox.Location = new System.Drawing.Point(153, 82);
            this.dBox.Name = "dBox";
            this.dBox.Size = new System.Drawing.Size(64, 64);
            this.dBox.TabIndex = 3;
            this.dBox.TabStop = false;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(11, 153);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(191, 24);
            this.checkBox1.TabIndex = 4;
            this.checkBox1.Text = "Window always on top";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // KeyPressGUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(227, 182);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.dBox);
            this.Controls.Add(this.sBox);
            this.Controls.Add(this.aBox);
            this.Controls.Add(this.wBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "KeyPressGUI";
            this.Text = "[FameBot] Keys";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.wBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.aBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox wBox;
        private System.Windows.Forms.PictureBox aBox;
        private System.Windows.Forms.PictureBox sBox;
        private System.Windows.Forms.PictureBox dBox;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}