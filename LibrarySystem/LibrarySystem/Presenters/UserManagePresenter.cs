using LibrarySystem.Services;
using LibrarySystem.Views;

namespace LibrarySystem.Presenters;

public class UserManagePresenter
{
    private readonly IUserManageView _view;
    private readonly LibraryService _libraryService;

    public UserManagePresenter(IUserManageView view, LibraryService libraryService)
    {
        _view = view;
        _libraryService = libraryService;
    }

    public void OnSearchClicked()
    {
        try
        {
            var userCode = _view.SearchUserCode.Trim();
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _view.ShowError("検索用の利用者コードを入力してください。");
                return;
            }

            var user = _libraryService.FindUserByCode(userCode);
            if (user is null)
            {
                _view.ShowError("利用者が見つかりません。");
                return;
            }

            _view.ShowUser(user);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    public void OnSaveClicked()
    {
        try
        {
            var user = _view.GetInputUser();
            if (string.IsNullOrWhiteSpace(user.UserCode))
            {
                _view.ShowError("利用者コードは必須です。");
                return;
            }

            if (string.IsNullOrWhiteSpace(user.FullName))
            {
                _view.ShowError("氏名は必須です。");
                return;
            }

            _libraryService.SaveUser(user);
            _view.ShowSuccess("利用者情報を保存しました。");
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    public void OnClearClicked()
    {
        _view.ClearForm();
    }
}
