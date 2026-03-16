using LibrarySystem.Common;
using LibrarySystem.Services;
using LibrarySystem.Views;

namespace LibrarySystem.Presenters;

public class LendingPresenter
{
    private readonly ILendingView _view;
    private readonly LibraryService _libraryService;

    public LendingPresenter(ILendingView view, LibraryService libraryService)
    {
        _view = view;
        _libraryService = libraryService;
    }

    public void OnLoadLendBookClicked()
    {
        try
        {
            if (!long.TryParse(_view.LendBookId, out var bookId))
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
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    public void OnLoadLendUserClicked()
    {
        try
        {
            var userCode = _view.LendUserCode.Trim();
            if (string.IsNullOrWhiteSpace(userCode))
            {
                _view.ShowError("利用者コードを入力してください。");
                return;
            }

            var user = _libraryService.GetUserByCode(userCode);
            if (user is null)
            {
                _view.ShowError("利用者が見つかりません。");
                return;
            }

            var activeCount = _libraryService.GetUserActiveLendingCount(user.UserId);
            var hasOverdue = _libraryService.HasOverdue(user.UserId);
            _view.ShowUserInfo(user, activeCount, hasOverdue);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    public void OnLendClicked()
    {
        try
        {
            if (!long.TryParse(_view.LendBookId, out var bookId))
            {
                _view.ShowError("蔵書IDは数値で入力してください。");
                return;
            }

            var userCode = _view.LendUserCode.Trim();
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

            var user = _libraryService.GetUserByCode(userCode);
            if (user is not null && _libraryService.HasOverdue(user.UserId))
            {
                _view.ShowWarning("この利用者には延滞があります。注意して処理してください。");
            }

            _libraryService.LendBook(bookId, userCode, AppSession.CurrentLibrarianId.Value);
            _view.ShowSuccess("貸出を登録しました。");
            _view.ClearLendingInputs();
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    public void OnLoadReturnInfoClicked()
    {
        try
        {
            if (!long.TryParse(_view.ReturnBookId, out var bookId))
            {
                _view.ShowError("蔵書IDは数値で入力してください。");
                return;
            }

            var result = _libraryService.GetReturnTarget(bookId);
            _view.ShowReturnInfo(result.Book, result.Borrower);
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }

    public void OnReturnClicked()
    {
        try
        {
            if (!long.TryParse(_view.ReturnBookId, out var bookId))
            {
                _view.ShowError("蔵書IDは数値で入力してください。");
                return;
            }

            if (!AppSession.CurrentLibrarianId.HasValue)
            {
                _view.ShowError("ログインセッションが無効です。");
                return;
            }

            var notice = _libraryService.ReturnBook(bookId, AppSession.CurrentLibrarianId.Value);
            _view.ShowSuccess("返却処理が完了しました。");
            if (!string.IsNullOrWhiteSpace(notice))
            {
                _view.ShowReservationNotice(notice);
            }

            _view.ClearReturnInputs();
        }
        catch (Exception ex)
        {
            _view.ShowError(ex.Message);
        }
    }
}
