namespace LibrarySystem.Views;

public interface IMainView
{
    void ShowLibrarianName(string name);
    void OpenBookList();
    void OpenBookAdd();
    void OpenLending();
    void OpenReservation();
    void OpenUserManage();
    void ReturnToLogin();
}
