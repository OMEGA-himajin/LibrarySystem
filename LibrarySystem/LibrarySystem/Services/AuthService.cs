using LibrarySystem.Common;
using LibrarySystem.Models;
using LibrarySystem.Repositories;

namespace LibrarySystem.Services;

public class AuthService
{
    private readonly LibrarianRepository _librarianRepository;

    public AuthService(LibrarianRepository librarianRepository)
    {
        _librarianRepository = librarianRepository;
    }

    public Librarian? Authenticate(string librarianCode, string password)
    {
        var librarian = _librarianRepository.FindByCode(librarianCode);
        if (librarian is null || !librarian.IsActive)
        {
            return null;
        }

        return PasswordHasher.Verify(password, librarian.PasswordHash, librarian.PasswordSalt)
            ? librarian
            : null;
    }
}
