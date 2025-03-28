using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BCrypt.Net;

namespace EamBackOffice01.UserControllers;

public partial class LoginUserControl : UserControl {
    const string ADMIN_PASSWORD_HASH = "$2a$11$Nty0dH6if2Q.AY3YseO4D./2zTjdCfnk5UktkbfekQz8/npCrHknC";
                                    // AdoroGatinhosFofos_123

    AdminForm _mainForm;
    public LoginUserControl(AdminForm mainForm) {
        InitializeComponent();
        _mainForm = mainForm;
    }

    private void loginButton_Click(object sender, EventArgs e) {
        if (!BCrypt.Net.BCrypt.Verify(passwordTextBox.Text, ADMIN_PASSWORD_HASH)) {
            MessageBox.Show(
                "Wrong password!",
                "WRONG PASSWORD",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }
        var mainUserControl = new MainUserControl();
        _mainForm.ShowUserControl(mainUserControl);
    }

    private void toggleShowPassowrdPictureBox_Click(object sender, EventArgs e) {
        char passwordChar = passwordTextBox.PasswordChar;
        passwordTextBox.PasswordChar = passwordChar == '●' ? '\0' : '●';
        passwordChar = passwordTextBox.PasswordChar;

        toggleShowPassowrdPictureBox.Image = passwordChar == '●' ? Properties.Resources.show_password
                                                                 : Properties.Resources.hide_password;
    }

    private void LoginUserControl_Load(object sender, EventArgs e) {

    }

    private void LoginUserControl_Resize(object sender, EventArgs e) {
        loginPanel.Left = (this.ClientSize.Width - loginPanel.Width) / 2;
        loginPanel.Top = (this.ClientSize.Height - loginPanel.Height) / 2;
    }
}