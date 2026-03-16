using LibrarySystem.Models;

namespace LibrarySystem.Views;

public interface IBookAddView
{
    Book GetInputBook();
    void ShowValidationError(string message);
    void ShowSuccess(string message);
    void ClearForm();
}
