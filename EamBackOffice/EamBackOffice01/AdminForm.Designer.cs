    namespace EamBackOffice01 {
        partial class AdminForm {
            /// <summary>
            ///  Required designer variable.
            /// </summary>
            private System.ComponentModel.IContainer components = null;

            /// <summary>
            ///  Clean up any resources being used.
            /// </summary>
            /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
            protected override void Dispose(bool disposing) {
                if (disposing && (components != null)) {
                    components.Dispose();
                }
                base.Dispose(disposing);
            }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.headerPanel = new Panel();
            this.pictureBox1 = new PictureBox();
            this.mainPanel = new Panel();
            this.headerPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)this.pictureBox1).BeginInit();
            this.SuspendLayout();
            // 
            // headerPanel
            // 
            this.headerPanel.BackColor = Color.Transparent;
            this.headerPanel.Controls.Add(this.pictureBox1);
            this.headerPanel.Dock = DockStyle.Top;
            this.headerPanel.Location = new Point(0, 0);
            this.headerPanel.Name = "headerPanel";
            this.headerPanel.Size = new Size(834, 110);
            this.headerPanel.TabIndex = 0;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = Properties.Resources.logoEAM_light1;
            this.pictureBox1.Location = new Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new Size(419, 98);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // mainPanel
            // 
            this.mainPanel.BackColor = Color.Transparent;
            this.mainPanel.Dock = DockStyle.Fill;
            this.mainPanel.Location = new Point(0, 110);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new Size(834, 461);
            this.mainPanel.TabIndex = 1;
            // 
            // AdminForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.LightSlateGray;
            this.ClientSize = new Size(834, 571);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.headerPanel);
            this.MinimumSize = new Size(850, 610);
            this.Name = "AdminForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "EAM | Admin";
            this.Load += this.AdminForm_Load;
            this.headerPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)this.pictureBox1).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private Panel headerPanel;
            private Panel mainPanel;
        private PictureBox pictureBox1;
    }
    }
