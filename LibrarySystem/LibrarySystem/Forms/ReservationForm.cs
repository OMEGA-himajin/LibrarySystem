using LibrarySystem.Models;
using LibrarySystem.Presenters;
using LibrarySystem.Services;
using LibrarySystem.Views;
using LibrarySystem.Common;

namespace LibrarySystem.Forms;

public class ReservationForm : Form, IReservationView
{
    private readonly ReservationPresenter _presenter;

    private readonly TextBox txtBookId = new() { Width = 150 };
    private readonly TextBox txtUserCode = new() { Width = 150 };
    private readonly Label lblBookInfo = new() { AutoSize = true };
    private readonly Label lblExpectedDueDate = new() { AutoSize = true };
    private readonly Button btnReserve = new() { Text = "予約", Width = 100 };

    public ReservationForm(LibraryService libraryService)
    {
        _presenter = new ReservationPresenter(this, libraryService);

        Text = "予約";
        StartPosition = FormStartPosition.CenterParent;
        Width = 500;
        Height = 260;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 5
        };

        layout.Controls.Add(new Label { Text = "蔵書ID", AutoSize = true }, 0, 0);
        layout.Controls.Add(txtBookId, 1, 0);
        layout.Controls.Add(new Label { Text = "利用者コード", AutoSize = true }, 0, 1);
        layout.Controls.Add(txtUserCode, 1, 1);
        layout.Controls.Add(lblBookInfo, 1, 2);
        layout.Controls.Add(lblExpectedDueDate, 1, 3);
        layout.Controls.Add(btnReserve, 1, 4);
        Controls.Add(layout);

        Load += (_, _) =>
        {
            if (!AppSession.IsLoggedIn)
            {
                Close();
            }
        };
        txtBookId.Leave += (_, _) => _presenter.OnBookIdEntered();
        btnReserve.Click += (_, _) => _presenter.OnReserveClicked();
    }

    public string BookId => txtBookId.Text.Trim();
    public string UserCode => txtUserCode.Text.Trim();

    public void ShowBookInfo(Book book)
    {
        lblBookInfo.Text = $"{book.Title} / {book.Author} / 状態:{book.Status}";
    }

    public void ShowDueDate(DateTime? dueDate)
    {
        lblExpectedDueDate.Text = dueDate.HasValue
            ? $"予定返却日: {dueDate.Value:yyyy/MM/dd}"
            : "予定返却日: -";
    }

    public void ShowSuccess(string message)
    {
        MessageBox.Show(this, message, "予約", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(this, message, "予約", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public void ClearInputs()
    {
        txtBookId.Clear();
        txtUserCode.Clear();
        lblBookInfo.Text = string.Empty;
        lblExpectedDueDate.Text = string.Empty;
    }
}
