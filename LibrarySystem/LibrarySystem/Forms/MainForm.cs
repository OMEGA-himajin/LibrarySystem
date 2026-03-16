using LibrarySystem.Common;
using LibrarySystem.Presenters;
using LibrarySystem.Services;
using LibrarySystem.Views;

namespace LibrarySystem.Forms;

public class MainForm : Form, IMainView
{
    private readonly MainPresenter _presenter;
    private readonly LibraryService _libraryService;

    private readonly Label lblLibrarianName = new() { AutoSize = true };
    private readonly Button btnBookList = new() { Text = "蔵書一覧", Width = 180 };
    private readonly Button btnBookAdd = new() { Text = "蔵書追加", Width = 180 };
    private readonly Button btnLending = new() { Text = "貸出/返却", Width = 180 };
    private readonly Button btnReservation = new() { Text = "予約", Width = 180 };
    private readonly Button btnUserManage = new() { Text = "利用者管理", Width = 180 };
    private readonly Button btnLogout = new() { Text = "ログアウト", Width = 180 };

    public MainForm(LibraryService libraryService)
    {
        _libraryService = libraryService;
        _presenter = new MainPresenter(this);

        Text = "図書館管理システム - メイン";
        StartPosition = FormStartPosition.CenterParent;
        Width = 360;
        Height = 360;

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(16),
            WrapContents = false
        };

        panel.Controls.Add(lblLibrarianName);
        panel.Controls.Add(btnBookList);
        panel.Controls.Add(btnBookAdd);
        panel.Controls.Add(btnLending);
        panel.Controls.Add(btnReservation);
        panel.Controls.Add(btnUserManage);
        panel.Controls.Add(btnLogout);
        Controls.Add(panel);

        Load += (_, _) => _presenter.OnFormLoad();
        btnBookList.Click += (_, _) => _presenter.OnBookListClicked();
        btnBookAdd.Click += (_, _) => _presenter.OnBookAddClicked();
        btnLending.Click += (_, _) => _presenter.OnLendingClicked();
        btnReservation.Click += (_, _) => _presenter.OnReservationClicked();
        btnUserManage.Click += (_, _) => _presenter.OnUserManageClicked();
        btnLogout.Click += (_, _) => _presenter.OnLogoutClicked();
    }

    public void ShowLibrarianName(string name)
    {
        lblLibrarianName.Text = $"ログイン司書: {name}";
    }

    public void OpenBookList()
    {
        using var form = new BookListForm(_libraryService);
        form.ShowDialog(this);
    }

    public void OpenBookAdd()
    {
        using var form = new BookAddForm(_libraryService);
        form.ShowDialog(this);
    }

    public void OpenLending()
    {
        using var form = new LendingForm(_libraryService);
        form.ShowDialog(this);
    }

    public void OpenReservation()
    {
        using var form = new ReservationForm(_libraryService);
        form.ShowDialog(this);
    }

    public void OpenUserManage()
    {
        using var form = new UserManageForm(_libraryService);
        form.ShowDialog(this);
    }

    public void ReturnToLogin()
    {
        if (AppSession.IsLoggedIn)
        {
            AppSession.Clear();
        }

        Close();
    }
}
