using LibrarySystem.Models;
using LibrarySystem.Presenters;
using LibrarySystem.Services;
using LibrarySystem.Views;
using System.ComponentModel;
using LibrarySystem.Common;

namespace LibrarySystem.Forms;

public class BookListForm : Form, IBookListView
{
    private readonly BookListPresenter _presenter;

    private readonly TextBox txtKeyword = new() { Width = 200 };
    private readonly ComboBox cmbStatus = new() { Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Button btnSearch = new() { Text = "検索", Width = 80 };
    private readonly DataGridView dgvBooks = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
    private readonly Button btnPrevPage = new() { Text = "前へ", Width = 80 };
    private readonly Button btnNextPage = new() { Text = "次へ", Width = 80 };
    private readonly Label lblPageInfo = new() { AutoSize = true };

    public BookListForm(LibraryService libraryService)
    {
        _presenter = new BookListPresenter(this, libraryService);
        CurrentPage = 1;

        Text = "蔵書一覧";
        StartPosition = FormStartPosition.CenterParent;
        Width = 900;
        Height = 520;

        cmbStatus.Items.AddRange(
        [
            new KeyValuePair<string, int?>("すべて", null),
            new KeyValuePair<string, int?>("在庫あり", 0),
            new KeyValuePair<string, int?>("貸出中", 1),
            new KeyValuePair<string, int?>("予約済", 2),
            new KeyValuePair<string, int?>("滞納中", 3)
        ]);
        cmbStatus.DisplayMember = "Key";
        cmbStatus.ValueMember = "Value";
        cmbStatus.SelectedIndex = 0;

        var topPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 45,
            Padding = new Padding(8)
        };
        topPanel.Controls.Add(new Label { Text = "キーワード", AutoSize = true, Margin = new Padding(3, 8, 3, 3) });
        topPanel.Controls.Add(txtKeyword);
        topPanel.Controls.Add(new Label { Text = "状態", AutoSize = true, Margin = new Padding(12, 8, 3, 3) });
        topPanel.Controls.Add(cmbStatus);
        topPanel.Controls.Add(btnSearch);

        var bottomPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 45,
            Padding = new Padding(8)
        };
        bottomPanel.Controls.Add(btnPrevPage);
        bottomPanel.Controls.Add(btnNextPage);
        bottomPanel.Controls.Add(lblPageInfo);

        Controls.Add(dgvBooks);
        Controls.Add(topPanel);
        Controls.Add(bottomPanel);

        Load += (_, _) =>
        {
            if (!AppSession.IsLoggedIn)
            {
                Close();
                return;
            }

            _presenter.OnFormLoad();
        };
        btnSearch.Click += (_, _) => _presenter.OnSearchClicked();
        btnPrevPage.Click += (_, _) => _presenter.OnPrevPageClicked();
        btnNextPage.Click += (_, _) => _presenter.OnNextPageClicked();
    }

    public string SearchKeyword => txtKeyword.Text.Trim();

    public int? SelectedStatus
    {
        get
        {
            if (cmbStatus.SelectedItem is KeyValuePair<string, int?> item)
            {
                return item.Value;
            }

            return null;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CurrentPage { get; set; }

    public void BindBooks(List<Book> books)
    {
        dgvBooks.DataSource = null;
        dgvBooks.DataSource = books;
    }

    public void ShowMessage(string message)
    {
        MessageBox.Show(this, message, "蔵書一覧", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ShowPageInfo(int currentPage, bool hasNextPage)
    {
        lblPageInfo.Text = $"ページ {currentPage}";
        btnPrevPage.Enabled = currentPage > 1;
        btnNextPage.Enabled = hasNextPage;
    }
}
