using LibrarySystem.Models;

namespace LibrarySystem.Views;

public interface ILendingView
{
    string LendBookId { get; }
    string LendUserCode { get; }
    string ReturnBookId { get; }
    void ShowBookInfo(Book book);
    void ShowUserInfo(User user, int activeLendingCount, bool hasOverdue);
    void ShowReturnInfo(Book book, User? borrower);
    void ShowWarning(string message);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowReservationNotice(string message);
    void ClearLendingInputs();
    void ClearReturnInputs();
}
