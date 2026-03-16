namespace LibrarySystem.Models;

public class Librarian
{
    public int LibrarianId { get; set; }
    public string LibrarianCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
