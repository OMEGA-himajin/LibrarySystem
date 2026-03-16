using LibrarySystem.Models;
using LibrarySystem.Presenters;
using LibrarySystem.Services;
using LibrarySystem.Views;
using LibrarySystem.Common;

namespace LibrarySystem.Forms;

public class BookAddForm : Form, IBookAddView
{
    private readonly BookAddPresenter _presenter;

    private readonly TextBox txtIsbn = new() { Width = 250 };
    private readonly TextBox txtTitle = new() { Width = 250 };
    private readonly TextBox txtAuthor = new() { Width = 250 };
    private readonly TextBox txtPublisher = new() { Width = 250 };
    private readonly TextBox txtPublishedYear = new() { Width = 100 };
    private readonly TextBox txtGenre = new() { Width = 250 };
    private readonly TextBox txtDescription = new() { Width = 350, Multiline = true, Height = 80 };
    private readonly Button btnSave = new() { Text = "保存", Width = 100 };
    private readonly Button btnClear = new() { Text = "クリア", Width = 100 };

    public BookAddForm(LibraryService libraryService)
    {
        _presenter = new BookAddPresenter(this, libraryService);

        Text = "蔵書追加";
        StartPosition = FormStartPosition.CenterParent;
        Width = 520;
        Height = 460;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 8
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label { Text = "ISBN", AutoSize = true }, 0, 0);
        layout.Controls.Add(txtIsbn, 1, 0);
        layout.Controls.Add(new Label { Text = "書名 *", AutoSize = true }, 0, 1);
        layout.Controls.Add(txtTitle, 1, 1);
        layout.Controls.Add(new Label { Text = "著者 *", AutoSize = true }, 0, 2);
        layout.Controls.Add(txtAuthor, 1, 2);
        layout.Controls.Add(new Label { Text = "出版社", AutoSize = true }, 0, 3);
        layout.Controls.Add(txtPublisher, 1, 3);
        layout.Controls.Add(new Label { Text = "出版年", AutoSize = true }, 0, 4);
        layout.Controls.Add(txtPublishedYear, 1, 4);
        layout.Controls.Add(new Label { Text = "ジャンル", AutoSize = true }, 0, 5);
        layout.Controls.Add(txtGenre, 1, 5);
        layout.Controls.Add(new Label { Text = "説明", AutoSize = true }, 0, 6);
        layout.Controls.Add(txtDescription, 1, 6);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill };
        buttons.Controls.Add(btnSave);
        buttons.Controls.Add(btnClear);
        layout.Controls.Add(buttons, 1, 7);

        Controls.Add(layout);

        Load += (_, _) =>
        {
            if (!AppSession.IsLoggedIn)
            {
                Close();
            }
        };
        btnSave.Click += (_, _) => _presenter.OnSaveClicked();
        btnClear.Click += (_, _) => _presenter.OnClearClicked();
    }

    public Book GetInputBook()
    {
        short? publishedYear = null;
        var yearText = txtPublishedYear.Text.Trim();
        if (!string.IsNullOrEmpty(yearText))
        {
            if (!short.TryParse(yearText, out var year))
            {
                throw new ArgumentException("出版年は数値で入力してください。");
            }

            publishedYear = year;
        }

        return new Book
        {
            ISBN = NullIfEmpty(txtIsbn.Text),
            Title = txtTitle.Text.Trim(),
            Author = txtAuthor.Text.Trim(),
            Publisher = NullIfEmpty(txtPublisher.Text),
            PublishedYear = publishedYear,
            Genre = NullIfEmpty(txtGenre.Text),
            Description = NullIfEmpty(txtDescription.Text),
            IsDeleted = false,
            Status = 0
        };
    }

    public void ShowValidationError(string message)
    {
        MessageBox.Show(this, message, "蔵書追加", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void ShowSuccess(string message)
    {
        MessageBox.Show(this, message, "蔵書追加", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ClearForm()
    {
        txtIsbn.Clear();
        txtTitle.Clear();
        txtAuthor.Clear();
        txtPublisher.Clear();
        txtPublishedYear.Clear();
        txtGenre.Clear();
        txtDescription.Clear();
        txtTitle.Focus();
    }

    private static string? NullIfEmpty(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
