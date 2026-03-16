using LibrarySystem.Forms;
using LibrarySystem.Repositories;
using LibrarySystem.Services;

namespace LibrarySystem;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var librarianRepository = new LibrarianRepository();
        var userRepository = new UserRepository();
        var bookRepository = new BookRepository();
        var lendingRepository = new LendingRepository();

        var authService = new AuthService(librarianRepository);
        var libraryService = new LibraryService(bookRepository, userRepository, lendingRepository);

        using var loginForm = new LoginForm(authService, () => new MainForm(libraryService));
        Application.Run(loginForm);
    }
}
