using LibrarySystem.Common;
using LibrarySystem.Models;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.Repositories;

public class BookRepository
{
    public Book? FindById(long bookId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        const string sql = """
            SELECT b.BookId, b.ISBN, b.Title, b.Author, b.Publisher, b.PublishedYear, b.Genre, b.Description, b.IsDeleted,
                   COALESCE(bs.Status, 0) AS Status, bs.CurrentLendingId, l.DueDate AS CurrentDueDate
            FROM Books b
            LEFT JOIN BookStatus bs ON bs.BookId = b.BookId
            LEFT JOIN Lendings l ON l.LendingId = bs.CurrentLendingId AND l.ReturnedAt IS NULL
            WHERE b.BookId = @BookId AND b.IsDeleted = 0
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return MapBook(reader);
    }

    public List<Book> Search(string keyword, int? status, int page, int pageSize)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        var books = new List<Book>();
        var offset = Math.Max(page - 1, 0) * pageSize;

        const string sql = """
            SELECT b.BookId, b.ISBN, b.Title, b.Author, b.Publisher, b.PublishedYear, b.Genre, b.Description, b.IsDeleted,
                   COALESCE(bs.Status, 0) AS Status, bs.CurrentLendingId, l.DueDate AS CurrentDueDate
            FROM Books b
            LEFT JOIN BookStatus bs ON bs.BookId = b.BookId
            LEFT JOIN Lendings l ON l.LendingId = bs.CurrentLendingId AND l.ReturnedAt IS NULL
            WHERE b.IsDeleted = 0
              AND (@Keyword = '' OR b.Title LIKE '%' + @Keyword + '%' OR b.Author LIKE '%' + @Keyword + '%' OR b.ISBN LIKE '%' + @Keyword + '%')
              AND (@Status IS NULL OR COALESCE(bs.Status, 0) = @Status)
            ORDER BY b.BookId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Keyword", keyword ?? string.Empty);
        cmd.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Offset", offset);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            books.Add(MapBook(reader));
        }

        return books;
    }

    public void Add(Book book)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        using var tx = conn.BeginTransaction();
        try
        {
            const string insertBookSql = """
                INSERT INTO Books (ISBN, Title, Author, Publisher, PublishedYear, Genre, Description, IsDeleted)
                OUTPUT INSERTED.BookId
                VALUES (@ISBN, @Title, @Author, @Publisher, @PublishedYear, @Genre, @Description, 0)
                """;

            long newBookId;
            using (var cmd = new SqlCommand(insertBookSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@ISBN", (object?)book.ISBN ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Title", book.Title);
                cmd.Parameters.AddWithValue("@Author", book.Author);
                cmd.Parameters.AddWithValue("@Publisher", (object?)book.Publisher ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PublishedYear", (object?)book.PublishedYear ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Genre", (object?)book.Genre ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object?)book.Description ?? DBNull.Value);
                newBookId = Convert.ToInt64(cmd.ExecuteScalar()!);
            }

            const string insertStatusSql = """
                INSERT INTO BookStatus (BookId, Status, CurrentLendingId)
                VALUES (@BookId, 0, NULL)
                """;
            using (var cmd = new SqlCommand(insertStatusSql, conn, tx))
            {
                cmd.Parameters.AddWithValue("@BookId", newBookId);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public void MarkLent(long bookId, long lendingId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        MarkLent(bookId, lendingId, conn, null);
    }

    public void MarkLent(long bookId, long lendingId, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            UPDATE BookStatus
            SET Status = 1,
                CurrentLendingId = @CurrentLendingId
            WHERE BookId = @BookId
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        cmd.Parameters.AddWithValue("@CurrentLendingId", lendingId);
        cmd.ExecuteNonQuery();
    }

    public void MarkAvailable(long bookId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        MarkAvailable(bookId, conn, null);
    }

    public void MarkAvailable(long bookId, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            UPDATE BookStatus
            SET Status = 0,
                CurrentLendingId = NULL
            WHERE BookId = @BookId
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        cmd.ExecuteNonQuery();
    }

    public void MarkReserved(long bookId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        MarkReserved(bookId, conn, null);
    }

    public void MarkReserved(long bookId, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            UPDATE BookStatus
            SET Status = 2,
                CurrentLendingId = NULL
            WHERE BookId = @BookId
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        cmd.ExecuteNonQuery();
    }

    private static Book MapBook(SqlDataReader reader)
    {
        return new Book
        {
            BookId = reader.GetInt64(reader.GetOrdinal("BookId")),
            ISBN = reader.IsDBNull(reader.GetOrdinal("ISBN")) ? null : reader.GetString(reader.GetOrdinal("ISBN")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Author = reader.GetString(reader.GetOrdinal("Author")),
            Publisher = reader.IsDBNull(reader.GetOrdinal("Publisher")) ? null : reader.GetString(reader.GetOrdinal("Publisher")),
            PublishedYear = reader.IsDBNull(reader.GetOrdinal("PublishedYear")) ? null : reader.GetInt16(reader.GetOrdinal("PublishedYear")),
            Genre = reader.IsDBNull(reader.GetOrdinal("Genre")) ? null : reader.GetString(reader.GetOrdinal("Genre")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
            Status = reader.GetInt32(reader.GetOrdinal("Status")),
            CurrentLendingId = reader.IsDBNull(reader.GetOrdinal("CurrentLendingId")) ? null : reader.GetInt64(reader.GetOrdinal("CurrentLendingId")),
            CurrentDueDate = reader.IsDBNull(reader.GetOrdinal("CurrentDueDate")) ? null : reader.GetDateTime(reader.GetOrdinal("CurrentDueDate"))
        };
    }
}
