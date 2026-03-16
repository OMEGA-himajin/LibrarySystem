using LibrarySystem.Models;

namespace LibrarySystem.Views;

public interface IReservationView
{
    string BookId { get; }
    string UserCode { get; }
    void ShowBookInfo(Book book);
    void ShowDueDate(DateTime? dueDate);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ClearInputs();
}
