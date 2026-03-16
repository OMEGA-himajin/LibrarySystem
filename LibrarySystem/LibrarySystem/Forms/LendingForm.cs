using LibrarySystem.Models;
using LibrarySystem.Presenters;
using LibrarySystem.Services;
using LibrarySystem.Views;
using LibrarySystem.Common;

namespace LibrarySystem.Forms;

public class LendingForm : Form, ILendingView
{
    private readonly LendingPresenter _presenter;

    private readonly TextBox txtLendBookId = new() { Width = 140 };
    private readonly TextBox txtLendUserCode = new() { Width = 140 };
    private readonly Label lblBookTitle = new() { AutoSize = true };
    private readonly Label lblBookStatus = new() { AutoSize = true };
    private readonly Label lblDueDate = new() { AutoSize = true };
    private readonly Label lblUserName = new() { AutoSize = true };
    private readonly Label lblActiveCount = new() { AutoSize = true };
    private readonly Label lblOverdueWarning = new() { AutoSize = true, ForeColor = Color.DarkRed };
    private readonly Button btnLoadBook = new() { Text = "本を読込", Width = 110 };
    private readonly Button btnLoadUser = new() { Text = "利用者読込", Width = 110 };
    private readonly Button btnLend = new() { Text = "貸出実行", Width = 120 };

    private readonly TextBox txtReturnBookId = new() { Width = 140 };
    private readonly Label lblReturnBookInfo = new() { AutoSize = true };
    private readonly Label lblBorrowerInfo = new() { AutoSize = true };
    private readonly Label lblReservationNotice = new() { AutoSize = true, ForeColor = Color.DarkBlue };
    private readonly Button btnLoadReturnInfo = new() { Text = "返却情報読込", Width = 120 };
    private readonly Button btnReturn = new() { Text = "返却実行", Width = 120 };

    public LendingForm(LibraryService libraryService)
    {
        _presenter = new LendingPresenter(this, libraryService);

        Text = "貸出 / 返却";
        StartPosition = FormStartPosition.CenterParent;
        Width = 700;
        Height = 450;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateLendingTab());
        tabs.TabPages.Add(CreateReturnTab());
        Controls.Add(tabs);

        Load += (_, _) =>
        {
            if (!AppSession.IsLoggedIn)
            {
                Close();
            }
        };
        btnLoadBook.Click += (_, _) => _presenter.OnLoadLendBookClicked();
        btnLoadUser.Click += (_, _) => _presenter.OnLoadLendUserClicked();
        btnLend.Click += (_, _) => _presenter.OnLendClicked();
        btnLoadReturnInfo.Click += (_, _) => _presenter.OnLoadReturnInfoClicked();
        btnReturn.Click += (_, _) => _presenter.OnReturnClicked();
    }

    public string LendBookId => txtLendBookId.Text.Trim();
    public string LendUserCode => txtLendUserCode.Text.Trim();
    public string ReturnBookId => txtReturnBookId.Text.Trim();

    public void ShowBookInfo(Book book)
    {
        lblBookTitle.Text = $"書名: {book.Title}";
        lblBookStatus.Text = $"状態: {book.Status}";
        lblDueDate.Text = $"返却期限: {(book.CurrentDueDate?.ToString("yyyy/MM/dd") ?? "-")}";
    }

    public void ShowUserInfo(User user, int activeLendingCount, bool hasOverdue)
    {
        lblUserName.Text = $"利用者: {user.FullName} ({user.UserCode})";
        lblActiveCount.Text = $"貸出中冊数: {activeLendingCount}";
        lblOverdueWarning.Text = hasOverdue ? "延滞あり" : "延滞なし";
    }

    public void ShowReturnInfo(Book book, User? borrower)
    {
        lblReturnBookInfo.Text = $"対象本: {book.Title} (状態:{book.Status})";
        lblBorrowerInfo.Text = borrower is null
            ? "貸出者: なし"
            : $"貸出者: {borrower.FullName} ({borrower.UserCode})";
    }

    public void ShowWarning(string message)
    {
        MessageBox.Show(this, message, "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void ShowSuccess(string message)
    {
        MessageBox.Show(this, message, "貸出/返却", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(this, message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ShowReservationNotice(string message)
    {
        lblReservationNotice.Text = message;
    }

    public void ClearLendingInputs()
    {
        txtLendBookId.Clear();
        txtLendUserCode.Clear();
        lblBookTitle.Text = string.Empty;
        lblBookStatus.Text = string.Empty;
        lblDueDate.Text = string.Empty;
        lblUserName.Text = string.Empty;
        lblActiveCount.Text = string.Empty;
        lblOverdueWarning.Text = string.Empty;
    }

    public void ClearReturnInputs()
    {
        txtReturnBookId.Clear();
        lblReturnBookInfo.Text = string.Empty;
        lblBorrowerInfo.Text = string.Empty;
    }

    private TabPage CreateLendingTab()
    {
        var tab = new TabPage("貸出");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 3,
            RowCount = 6
        };

        layout.Controls.Add(new Label { Text = "蔵書ID", AutoSize = true }, 0, 0);
        layout.Controls.Add(txtLendBookId, 1, 0);
        layout.Controls.Add(btnLoadBook, 2, 0);

        layout.Controls.Add(new Label { Text = "利用者コード", AutoSize = true }, 0, 1);
        layout.Controls.Add(txtLendUserCode, 1, 1);
        layout.Controls.Add(btnLoadUser, 2, 1);

        layout.Controls.Add(lblBookTitle, 1, 2);
        layout.Controls.Add(lblBookStatus, 1, 3);
        layout.Controls.Add(lblDueDate, 1, 4);
        layout.Controls.Add(lblUserName, 1, 5);
        layout.Controls.Add(lblActiveCount, 2, 5);
        layout.Controls.Add(lblOverdueWarning, 1, 6);
        layout.Controls.Add(btnLend, 2, 6);

        tab.Controls.Add(layout);
        return tab;
    }

    private TabPage CreateReturnTab()
    {
        var tab = new TabPage("返却");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 3,
            RowCount = 5
        };

        layout.Controls.Add(new Label { Text = "蔵書ID", AutoSize = true }, 0, 0);
        layout.Controls.Add(txtReturnBookId, 1, 0);
        layout.Controls.Add(btnLoadReturnInfo, 2, 0);

        layout.Controls.Add(lblReturnBookInfo, 1, 1);
        layout.Controls.Add(lblBorrowerInfo, 1, 2);
        layout.Controls.Add(lblReservationNotice, 1, 3);
        layout.Controls.Add(btnReturn, 2, 4);

        tab.Controls.Add(layout);
        return tab;
    }
}
