using LibrarySystem.Common;
using LibrarySystem.Models;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.Repositories;

public class LendingRepository
{
    public long AddLending(Lending lending)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        return AddLending(lending, conn, null);
    }

    public long AddLending(Lending lending, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            INSERT INTO Lendings (BookId, UserId, LibrarianId, LentAt, DueDate, ReturnedAt, ReturnLibrarianId)
            OUTPUT INSERTED.LendingId
            VALUES (@BookId, @UserId, @LibrarianId, @LentAt, @DueDate, NULL, NULL)
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@BookId", lending.BookId);
        cmd.Parameters.AddWithValue("@UserId", lending.UserId);
        cmd.Parameters.AddWithValue("@LibrarianId", lending.LibrarianId);
        cmd.Parameters.AddWithValue("@LentAt", lending.LentAt);
        cmd.Parameters.AddWithValue("@DueDate", lending.DueDate);

        return Convert.ToInt64(cmd.ExecuteScalar()!);
    }

    public int CountActiveLendingsByUserId(int userId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        const string sql = """
            SELECT COUNT(1)
            FROM Lendings
            WHERE UserId = @UserId AND ReturnedAt IS NULL
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        return Convert.ToInt32(cmd.ExecuteScalar()!);
    }

    public bool ExistsOverdueByUserId(int userId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1
                FROM Lendings
                WHERE UserId = @UserId AND ReturnedAt IS NULL AND DueDate < @Now
            ) THEN 1 ELSE 0 END
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserId", userId);
        cmd.Parameters.AddWithValue("@Now", DateTime.Now);
        return Convert.ToInt32(cmd.ExecuteScalar()!) == 1;
    }

    public Lending? GetActiveLendingByBookId(long bookId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        return GetActiveLendingByBookId(bookId, conn, null);
    }

    public Lending? GetActiveLendingByBookId(long bookId, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            SELECT TOP 1 LendingId, BookId, UserId, LibrarianId, LentAt, DueDate, ReturnedAt, ReturnLibrarianId
            FROM Lendings
            WHERE BookId = @BookId AND ReturnedAt IS NULL
            ORDER BY LentAt DESC
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new Lending
        {
            LendingId = reader.GetInt64(reader.GetOrdinal("LendingId")),
            BookId = reader.GetInt64(reader.GetOrdinal("BookId")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            LibrarianId = reader.GetInt32(reader.GetOrdinal("LibrarianId")),
            LentAt = reader.GetDateTime(reader.GetOrdinal("LentAt")),
            DueDate = reader.GetDateTime(reader.GetOrdinal("DueDate")),
            ReturnedAt = null,
            ReturnLibrarianId = reader.IsDBNull(reader.GetOrdinal("ReturnLibrarianId"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("ReturnLibrarianId"))
        };
    }

    public User? GetBorrowerByBookId(long bookId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        const string sql = """
            SELECT TOP 1 u.UserId, u.UserCode, u.FullName, u.BirthDate, u.Gender, u.PhoneNumber, u.Email, u.IsActive
            FROM Lendings l
            INNER JOIN Users u ON u.UserId = l.UserId
            WHERE l.BookId = @BookId AND l.ReturnedAt IS NULL
            ORDER BY l.LentAt DESC
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new User
        {
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            UserCode = reader.GetString(reader.GetOrdinal("UserCode")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate")),
            Gender = reader.GetByte(reader.GetOrdinal("Gender")),
            PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
        };
    }

    public void ReturnBook(long bookId, int returnLibrarianId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        ReturnBook(bookId, returnLibrarianId, conn, null);
    }

    public void ReturnBook(long bookId, int returnLibrarianId, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            UPDATE Lendings
            SET ReturnedAt = @ReturnedAt,
                ReturnLibrarianId = @ReturnLibrarianId
            WHERE BookId = @BookId AND ReturnedAt IS NULL
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        cmd.Parameters.AddWithValue("@ReturnedAt", DateTime.Now);
        cmd.Parameters.AddWithValue("@ReturnLibrarianId", returnLibrarianId);
        cmd.ExecuteNonQuery();
    }

    public Reservation? GetActiveReservationByBookId(long bookId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        return GetActiveReservationByBookId(bookId, conn, null);
    }

    public Reservation? GetActiveReservationByBookId(long bookId, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            SELECT TOP 1 ReservationId, BookId, UserId, LibrarianId, ReservedAt, Status, NotifiedAt, ExpiresAt
            FROM Reservations
            WHERE BookId = @BookId AND Status = 0
            ORDER BY ReservedAt ASC
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return MapReservation(reader);
    }

    public Reservation? GetActiveReservationByBookIdExcludingUser(long bookId, int userId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        const string sql = """
            SELECT TOP 1 ReservationId, BookId, UserId, LibrarianId, ReservedAt, Status, NotifiedAt, ExpiresAt
            FROM Reservations
            WHERE BookId = @BookId AND Status = 0 AND UserId <> @UserId
            ORDER BY ReservedAt ASC
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return MapReservation(reader);
    }

    public bool ExistsActiveReservationByBookIdAndUserId(long bookId, int userId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM Reservations
                WHERE BookId = @BookId AND UserId = @UserId AND Status = 0
            ) THEN 1 ELSE 0 END
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@BookId", bookId);
        cmd.Parameters.AddWithValue("@UserId", userId);
        return Convert.ToInt32(cmd.ExecuteScalar()!) == 1;
    }

    public void AddReservation(Reservation reservation)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        AddReservation(reservation, conn, null);
    }

    public void AddReservation(Reservation reservation, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            INSERT INTO Reservations (BookId, UserId, LibrarianId, ReservedAt, Status, NotifiedAt, ExpiresAt)
            VALUES (@BookId, @UserId, @LibrarianId, @ReservedAt, @Status, @NotifiedAt, @ExpiresAt)
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@BookId", reservation.BookId);
        cmd.Parameters.AddWithValue("@UserId", reservation.UserId);
        cmd.Parameters.AddWithValue("@LibrarianId", reservation.LibrarianId);
        cmd.Parameters.AddWithValue("@ReservedAt", reservation.ReservedAt);
        cmd.Parameters.AddWithValue("@Status", reservation.Status);
        cmd.Parameters.AddWithValue("@NotifiedAt", (object?)reservation.NotifiedAt ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ExpiresAt", (object?)reservation.ExpiresAt ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void CompleteReservation(long reservationId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        CompleteReservation(reservationId, conn, null);
    }

    public void CompleteReservation(long reservationId, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            UPDATE Reservations
            SET Status = 1
            WHERE ReservationId = @ReservationId
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@ReservationId", reservationId);
        cmd.ExecuteNonQuery();
    }

    public void SetReservationHold(long reservationId, DateTime expiresAt)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        SetReservationHold(reservationId, expiresAt, conn, null);
    }

    public void SetReservationHold(long reservationId, DateTime expiresAt, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            UPDATE Reservations
            SET NotifiedAt = @NotifiedAt,
                ExpiresAt = @ExpiresAt
            WHERE ReservationId = @ReservationId
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@ReservationId", reservationId);
        cmd.Parameters.AddWithValue("@NotifiedAt", DateTime.Now);
        cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
        cmd.ExecuteNonQuery();
    }

    private static Reservation MapReservation(SqlDataReader reader)
    {
        return new Reservation
        {
            ReservationId = reader.GetInt64(reader.GetOrdinal("ReservationId")),
            BookId = reader.GetInt64(reader.GetOrdinal("BookId")),
            UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
            LibrarianId = reader.GetInt32(reader.GetOrdinal("LibrarianId")),
            ReservedAt = reader.GetDateTime(reader.GetOrdinal("ReservedAt")),
            Status = reader.GetByte(reader.GetOrdinal("Status")),
            NotifiedAt = reader.IsDBNull(reader.GetOrdinal("NotifiedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("NotifiedAt")),
            ExpiresAt = reader.IsDBNull(reader.GetOrdinal("ExpiresAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ExpiresAt"))
        };
    }
}
