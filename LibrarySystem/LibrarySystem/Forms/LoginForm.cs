using LibrarySystem.Presenters;
using LibrarySystem.Services;
using LibrarySystem.Views;

namespace LibrarySystem.Forms;

public class LoginForm : Form, ILoginView
{
    private readonly LoginPresenter _presenter;
    private readonly Func<Form> _mainFormFactory;

    private readonly TextBox txtLibrarianCode = new() { Width = 220 };
    private readonly TextBox txtPassword = new() { Width = 220, UseSystemPasswordChar = true };
    private readonly Button btnLogin = new() { Text = "ログイン", Width = 120 };
    private readonly Label lblError = new() { AutoSize = true, ForeColor = Color.Red };

    public LoginForm(AuthService authService, Func<Form> mainFormFactory)
    {
        _mainFormFactory = mainFormFactory;
        _presenter = new LoginPresenter(this, authService);

        Text = "司書ログイン";
        StartPosition = FormStartPosition.CenterScreen;
        Width = 420;
        Height = 240;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(16)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label { Text = "司書コード", AutoSize = true }, 0, 0);
        layout.Controls.Add(txtLibrarianCode, 1, 0);
        layout.Controls.Add(new Label { Text = "パスワード", AutoSize = true }, 0, 1);
        layout.Controls.Add(txtPassword, 1, 1);
        layout.Controls.Add(btnLogin, 1, 2);
        layout.Controls.Add(lblError, 1, 3);
        Controls.Add(layout);

        btnLogin.Click += (_, _) => _presenter.OnLoginClicked();
        AcceptButton = btnLogin;
    }

    public string LibrarianCode => txtLibrarianCode.Text;
    public string Password => txtPassword.Text;

    public void ShowError(string message)
    {
        lblError.Text = message;
    }

    public void ClearError()
    {
        lblError.Text = string.Empty;
    }

    public void NavigateToMain()
    {
        Hide();
        using var mainForm = _mainFormFactory();
        mainForm.ShowDialog(this);
        Show();
        txtPassword.Text = string.Empty;
        txtPassword.Focus();
    }
}
