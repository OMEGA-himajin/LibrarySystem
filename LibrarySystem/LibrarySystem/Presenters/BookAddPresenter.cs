using LibrarySystem.Services;
using LibrarySystem.Views;

namespace LibrarySystem.Presenters;

public class BookAddPresenter
{
    private readonly IBookAddView _view;
    private readonly LibraryService _libraryService;

    public BookAddPresenter(IBookAddView view, LibraryService libraryService)
    {
        _view = view;
        _libraryService = libraryService;
    }

    public void OnSaveClicked()
    {
        try
        {
            var book = _view.GetInputBook();
            ValidateBook(book);
            _libraryService.AddBook(book);
            _view.ShowSuccess("蔵書を登録しました。");
            _view.ClearForm();
        }
        catch (ArgumentException ex)
        {
            _view.ShowValidationError(ex.Message);
        }
        catch (Exception ex)
        {
            _view.ShowValidationError(ex.Message);
        }
    }

    public void OnClearClicked()
    {
        _view.ClearForm();
    }

    private static void ValidateBook(Models.Book book)
    {
        if (string.IsNullOrWhiteSpace(book.Title))
        {
            throw new ArgumentException("書名は必須です。");
        }

        if (string.IsNullOrWhiteSpace(book.Author))
        {
            throw new ArgumentException("著者は必須です。");
        }

        if (book.Title.Length > 200)
        {
            throw new ArgumentException("書名が長すぎます。");
        }

        if (book.Author.Length > 200)
        {
            throw new ArgumentException("著者名が長すぎます。");
        }
    }
}
