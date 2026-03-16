using LibrarySystem.Common;
using LibrarySystem.Services;
using LibrarySystem.Views;

namespace LibrarySystem.Presenters;

public class ReservationPresenter
{
    private readonly IReservationView _view;
    private readonly LibraryService _libraryService;

    public ReservationPresenter(IReservationView view, LibraryService libraryService)
    {
        _view = view;
        _libraryService = libraryService;
    }

    public void OnBookIdEntered()
    {
        try
        {
            if (!long.TryParse(_view.BookId, out var bookId))
            {
                _view.ShowError("蔵書IDは数値で入力してください。");
                return;
            }

            var book = _libraryService.GetBookForDisplay(bookId);
            if (book is null)
            {
                _view.ShowError("蔵書が見つかりません。");
                return;
            }

            _view.ShowBookInfo(book);
            _view.ShowDueDate(book.CurrentDueDate);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    public void OnReserveClicked()
    {
        try
        {
            if (!long.TryParse(_view.BookId, out var bookId))
            {
                _view.ShowError("蔵書IDは数値で入力してください。");
                return;
            }

            var userCode = _view.UserCode.Trim();
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _view.ShowError("利用者コードを入力してください。");
                return;
            }

            if (!AppSession.CurrentLibrarianId.HasValue)
            {
                _view.ShowError("ログインセッションが無効です。");
                return;
            }

            _libraryService.ReserveBook(bookId, userCode, AppSession.CurrentLibrarianId.Value);
            _view.ShowSuccess("予約を登録しました。");
            _view.ClearInputs();
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }
}
