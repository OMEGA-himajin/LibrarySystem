using LibrarySystem.Models;

namespace LibrarySystem.Views;

public interface IBookListView
{
    string SearchKeyword { get; }
    int? SelectedStatus { get; }
    int CurrentPage { get; set; }
    void BindBooks(List<Book> books);
    void ShowMessage(string message);
    void ShowPageInfo(int currentPage, bool hasNextPage);
}
