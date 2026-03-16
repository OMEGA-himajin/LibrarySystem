using LibrarySystem.Models;
using LibrarySystem.Presenters;
using LibrarySystem.Services;
using LibrarySystem.Views;
using LibrarySystem.Common;

namespace LibrarySystem.Forms;

public class UserManageForm : Form, IUserManageView
{
    private readonly UserManagePresenter _presenter;
    private int _currentUserId;

    private readonly TextBox txtSearchUserCode = new() { Width = 140 };
    private readonly Button btnSearch = new() { Text = "検索", Width = 90 };

    private readonly TextBox txtUserCode = new() { Width = 180 };
    private readonly TextBox txtFullName = new() { Width = 180 };
    private readonly DateTimePicker dtpBirthDate = new() { Width = 140 };
    private readonly ComboBox cmbGender = new() { Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox txtPhoneNumber = new() { Width = 180 };
    private readonly TextBox txtEmail = new() { Width = 220 };
    private readonly CheckBox chkIsActive = new() { Text = "有効", Checked = true, AutoSize = true };
    private readonly Button btnSave = new() { Text = "保存", Width = 100 };
    private readonly Button btnClear = new() { Text = "クリア", Width = 100 };

    public UserManageForm(LibraryService libraryService)
    {
        _presenter = new UserManagePresenter(this, libraryService);

        Text = "利用者管理";
        StartPosition = FormStartPosition.CenterParent;
        Width = 580;
        Height = 420;

        cmbGender.Items.AddRange(
        [
            new KeyValuePair<string, byte>("未設定", 0),
            new KeyValuePair<string, byte>("男性", 1),
            new KeyValuePair<string, byte>("女性", 2)
        ]);
        cmbGender.DisplayMember = "Key";
        cmbGender.ValueMember = "Value";
        cmbGender.SelectedIndex = 0;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 9
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        var searchPanel = new FlowLayoutPanel { AutoSize = true };
        searchPanel.Controls.Add(txtSearchUserCode);
        searchPanel.Controls.Add(btnSearch);
        root.Controls.Add(new Label { Text = "検索用コード", AutoSize = true }, 0, 0);
        root.Controls.Add(searchPanel, 1, 0);
        root.Controls.Add(new Label { Text = "利用者コード *", AutoSize = true }, 0, 1);
        root.Controls.Add(txtUserCode, 1, 1);
        root.Controls.Add(new Label { Text = "氏名 *", AutoSize = true }, 0, 2);
        root.Controls.Add(txtFullName, 1, 2);
        root.Controls.Add(new Label { Text = "生年月日", AutoSize = true }, 0, 3);
        root.Controls.Add(dtpBirthDate, 1, 3);
        root.Controls.Add(new Label { Text = "性別", AutoSize = true }, 0, 4);
        root.Controls.Add(cmbGender, 1, 4);
        root.Controls.Add(new Label { Text = "電話番号", AutoSize = true }, 0, 5);
        root.Controls.Add(txtPhoneNumber, 1, 5);
        root.Controls.Add(new Label { Text = "メール", AutoSize = true }, 0, 6);
        root.Controls.Add(txtEmail, 1, 6);
        root.Controls.Add(new Label { Text = "状態", AutoSize = true }, 0, 7);
        root.Controls.Add(chkIsActive, 1, 7);

        var buttonPanel = new FlowLayoutPanel { AutoSize = true };
        buttonPanel.Controls.Add(btnSave);
        buttonPanel.Controls.Add(btnClear);
        root.Controls.Add(buttonPanel, 1, 8);

        Controls.Add(root);

        Load += (_, _) =>
        {
            if (!AppSession.IsLoggedIn)
            {
                Close();
            }
        };
        btnSearch.Click += (_, _) => _presenter.OnSearchClicked();
        btnSave.Click += (_, _) => _presenter.OnSaveClicked();
        btnClear.Click += (_, _) => _presenter.OnClearClicked();
    }

    public string SearchUserCode => txtSearchUserCode.Text.Trim();

    public User GetInputUser()
    {
        if (!string.IsNullOrWhiteSpace(txtEmail.Text))
        {
            try
            {
                _ = new System.Net.Mail.MailAddress(txtEmail.Text.Trim());
            }
            catch
            {
                throw new ArgumentException("メールアドレス形式が正しくありません。");
            }
        }

        return new User
        {
            UserId = _currentUserId,
            UserCode = txtUserCode.Text.Trim(),
            FullName = txtFullName.Text.Trim(),
            BirthDate = dtpBirthDate.Value.Date,
            Gender = cmbGender.SelectedItem is KeyValuePair<string, byte> g ? g.Value : (byte)0,
            PhoneNumber = NullIfEmpty(txtPhoneNumber.Text),
            Email = NullIfEmpty(txtEmail.Text),
            IsActive = chkIsActive.Checked
        };
    }

    public void ShowUser(User user)
    {
        _currentUserId = user.UserId;
        txtUserCode.Text = user.UserCode;
        txtFullName.Text = user.FullName;
        dtpBirthDate.Value = user.BirthDate;
        SelectGender(user.Gender);
        txtPhoneNumber.Text = user.PhoneNumber ?? string.Empty;
        txtEmail.Text = user.Email ?? string.Empty;
        chkIsActive.Checked = user.IsActive;
    }

    public void ShowSuccess(string message)
    {
        MessageBox.Show(this, message, "利用者管理", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(this, message, "利用者管理", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void ClearForm()
    {
        _currentUserId = 0;
        txtSearchUserCode.Clear();
        txtUserCode.Clear();
        txtFullName.Clear();
        dtpBirthDate.Value = DateTime.Today;
        cmbGender.SelectedIndex = 0;
        txtPhoneNumber.Clear();
        txtEmail.Clear();
        chkIsActive.Checked = true;
    }

    private void SelectGender(byte gender)
    {
        for (var i = 0; i < cmbGender.Items.Count; i++)
        {
            if (cmbGender.Items[i] is KeyValuePair<string, byte> item && item.Value == gender)
            {
                cmbGender.SelectedIndex = i;
                return;
            }
        }

        cmbGender.SelectedIndex = 0;
    }

    private static string? NullIfEmpty(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}
