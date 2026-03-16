using LibrarySystem.Models;

namespace LibrarySystem.Common;

public static class AppSession
{
    public static int? CurrentLibrarianId { get; private set; }
    public static string? CurrentLibrarianName { get; private set; }
    public static bool IsLoggedIn => CurrentLibrarianId.HasValue;

    public static void SetCurrentLibrarian(Librarian librarian)
    {
        CurrentLibrarianId = librarian.LibrarianId;
        CurrentLibrarianName = librarian.FullName;
    }

    public static void Clear()
    {
        CurrentLibrarianId = null;
        CurrentLibrarianName = null;
    }
}
