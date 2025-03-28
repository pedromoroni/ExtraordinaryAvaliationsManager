using EamBackOffice01.UserControllers;

namespace EamBackOffice01;

public partial class AdminForm : Form {
    public AdminForm() {
        InitializeComponent();
        var loginUserControl = new LoginUserControl(this);
        ShowUserControl(loginUserControl);
    }

    #region Helpers
    public void ShowUserControl(UserControl userControl) {
        userControl.Dock = DockStyle.Fill;
        mainPanel.Controls.Clear();
        mainPanel.Controls.Add(userControl);
        userControl.BringToFront();
    }
    #endregion

    private void AdminForm_Load(object sender, EventArgs e) {
    }
}