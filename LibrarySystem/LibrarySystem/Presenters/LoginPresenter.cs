using LibrarySystem.Common;
using LibrarySystem.Services;
using LibrarySystem.Views;

namespace LibrarySystem.Presenters;

public class LoginPresenter
{
    private readonly ILoginView _view;
    private readonly AuthService _authService;

    public LoginPresenter(ILoginView view, AuthService authService)
    {
        _view = view;
        _authService = authService;
    }

    public void OnLoginClicked()
    {
        try
        {
            _view.ClearError();
            var librarianCode = _view.LibrarianCode.Trim();
            var password = _view.Password;

            if (string.IsNullOrWhiteSpace(librarianCode))
            {
                _view.ShowError("司書コードを入力してください。");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _view.ShowError("パスワードを入力してください。");
                return;
            }

            var librarian = _authService.Authenticate(librarianCode, password);
            if (librarian is null)
            {
                _view.ShowError("司書コードまたはパスワードが正しくありません。");
                return;
            }

            AppSession.SetCurrentLibrarian(librarian);
            _view.NavigateToMain();
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }
}
