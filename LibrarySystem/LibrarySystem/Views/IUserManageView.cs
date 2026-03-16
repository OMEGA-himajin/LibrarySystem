using LibrarySystem.Models;

namespace LibrarySystem.Views;

public interface IUserManageView
{
    string SearchUserCode { get; }
    User GetInputUser();
    void ShowUser(User user);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ClearForm();
}
