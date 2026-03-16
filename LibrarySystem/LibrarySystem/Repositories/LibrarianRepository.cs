using LibrarySystem.Common;
using LibrarySystem.Models;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.Repositories;

public class LibrarianRepository
{
    public Librarian? FindByCode(string librarianCode)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        const string sql = """
            SELECT LibrarianId, LibrarianCode, FullName, PasswordHash, PasswordSalt, IsActive
            FROM Librarians
            WHERE LibrarianCode = @LibrarianCode AND IsActive = 1
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@LibrarianCode", librarianCode);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Librarian
        {
            LibrarianId = reader.GetInt32(reader.GetOrdinal("LibrarianId")),
            LibrarianCode = reader.GetString(reader.GetOrdinal("LibrarianCode")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            PasswordSalt = reader.GetString(reader.GetOrdinal("PasswordSalt")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }
}
