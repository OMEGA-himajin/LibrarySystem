using LibrarySystem.Services;
using LibrarySystem.Views;

namespace LibrarySystem.Presenters;

public class BookListPresenter
{
    private readonly IBookListView _view;
    private readonly LibraryService _libraryService;
    private const int PageSize = 20;

    public BookListPresenter(IBookListView view, LibraryService libraryService)
    {
        _view = view;
        _libraryService = libraryService;
    }

    public void OnFormLoad()
    {
        _view.CurrentPage = 1;
        Search();
    }

    public void OnSearchClicked()
    {
        _view.CurrentPage = 1;
        Search();
    }

    public void OnPrevPageClicked()
    {
        if (_view.CurrentPage <= 1)
        {
            return;
        }

        _view.CurrentPage--;
        Search();
    }

    public void OnNextPageClicked()
    {
        _view.CurrentPage++;
        Search();
    }

    private void Search()
    {
        try
        {
            var books = _libraryService.SearchBooks(
                _view.SearchKeyword,
                _view.SelectedStatus,
                _view.CurrentPage,
                PageSize + 1);

            var hasNextPage = books.Count > PageSize;
            if (hasNextPage)
            {
                books.RemoveAt(books.Count - 1);
            }

            if (_view.CurrentPage > 1 && books.Count == 0)
            {
                _view.CurrentPage--;
                Search();
                return;
            }

            _view.BindBooks(books);
            _view.ShowPageInfo(_view.CurrentPage, hasNextPage);
        }
        catch (Exception ex)
        {
            _view.ShowMessage(ex.Message);
        }
    }
}
