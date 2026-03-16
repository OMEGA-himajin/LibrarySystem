using LibrarySystem.Common;
using LibrarySystem.Views;

namespace LibrarySystem.Presenters;

public class MainPresenter
{
    private readonly IMainView _view;

    public MainPresenter(IMainView view)
    {
        _view = view;
    }

    public void OnFormLoad()
    {
        if (!AppSession.IsLoggedIn)
        {
            _view.ReturnToLogin();
            return;
        }

        _view.ShowLibrarianName(AppSession.CurrentLibrarianName ?? string.Empty);
    }

    public void OnBookListClicked() => _view.OpenBookList();
    public void OnBookAddClicked() => _view.OpenBookAdd();
    public void OnLendingClicked() => _view.OpenLending();
    public void OnReservationClicked() => _view.OpenReservation();
    public void OnUserManageClicked() => _view.OpenUserManage();

    public void OnLogoutClicked()
    {
        AppSession.Clear();
        _view.ReturnToLogin();
    }
}
