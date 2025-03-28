namespace EamBackOffice01.UserControllers {
    partial class LoginUserControl {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.  
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            passwordTextBox = new TextBox();
            toggleShowPassowrdPictureBox = new PictureBox();
            loginButton = new Button();
            loginPanel = new Panel();
            passwordLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)toggleShowPassowrdPictureBox).BeginInit();
            loginPanel.SuspendLayout();
            SuspendLayout();
            // 
            // passwordTextBox
            // 
            passwordTextBox.BackColor = Color.White;
            passwordTextBox.BorderStyle = BorderStyle.FixedSingle;
            passwordTextBox.Font = new Font("Segoe UI", 14.25F);
            passwordTextBox.Location = new Point(85, 182);
            passwordTextBox.Name = "passwordTextBox";
            passwordTextBox.PasswordChar = '●';
            passwordTextBox.Size = new Size(321, 33);
            passwordTextBox.TabIndex = 1;
            // 
            // toggleShowPassowrdPictureBox
            // 
            toggleShowPassowrdPictureBox.BackColor = Color.Transparent;
            toggleShowPassowrdPictureBox.Image = Properties.Resources.show_password;
            toggleShowPassowrdPictureBox.Location = new Point(412, 182);
            toggleShowPassowrdPictureBox.Name = "toggleShowPassowrdPictureBox";
            toggleShowPassowrdPictureBox.Size = new Size(33, 33);
            toggleShowPassowrdPictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            toggleShowPassowrdPictureBox.TabIndex = 2;
            toggleShowPassowrdPictureBox.TabStop = false;
            toggleShowPassowrdPictureBox.Click += toggleShowPassowrdPictureBox_Click;
            // 
            // loginButton
            // 
            loginButton.BackColor = Color.White;
            loginButton.FlatStyle = FlatStyle.Flat;
            loginButton.Font = new Font("Segoe UI", 15.75F);
            loginButton.Location = new Point(137, 246);
            loginButton.Name = "loginButton";
            loginButton.Size = new Size(217, 50);
            loginButton.TabIndex = 3;
            loginButton.Text = "Login";
            loginButton.UseVisualStyleBackColor = false;
            loginButton.Click += loginButton_Click;
            // 
            // loginPanel
            // 
            loginPanel.Controls.Add(toggleShowPassowrdPictureBox);
            loginPanel.Controls.Add(passwordLabel);
            loginPanel.Controls.Add(loginButton);
            loginPanel.Controls.Add(passwordTextBox);
            loginPanel.Location = new Point(244, 97);
            loginPanel.Name = "loginPanel";
            loginPanel.Size = new Size(490, 443);
            loginPanel.TabIndex = 5;
            // 
            // passwordLabel
            // 
            passwordLabel.AutoSize = true;
            passwordLabel.Font = new Font("Segoe UI", 15.75F);
            passwordLabel.Location = new Point(196, 147);
            passwordLabel.Name = "passwordLabel";
            passwordLabel.Size = new Size(99, 30);
            passwordLabel.TabIndex = 5;
            passwordLabel.Text = "Password";
            // 
            // LoginUserControl
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.WhiteSmoke;
            Controls.Add(loginPanel);
            Name = "LoginUserControl";
            Size = new Size(978, 637);
            Load += LoginUserControl_Load;
            Resize += LoginUserControl_Resize;
            ((System.ComponentModel.ISupportInitialize)toggleShowPassowrdPictureBox).EndInit();
            loginPanel.ResumeLayout(false);
            loginPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private TextBox passwordTextBox;
        private PictureBox toggleShowPassowrdPictureBox;
        private Button loginButton;
        private Panel loginPanel;
        private Label passwordLabel;
    }
}
