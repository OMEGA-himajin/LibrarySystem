namespace LibrarySystem.Views;

public interface ILoginView
{
    string LibrarianCode { get; }
    string Password { get; }
    void ShowError(string message);
    void ClearError();
    void NavigateToMain();
}
