using LibrarySystem.Common;
using LibrarySystem.Models;
using Microsoft.Data.SqlClient;

namespace LibrarySystem.Repositories;

public class UserRepository
{
    public User? FindById(int userId)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        return FindById(userId, conn, null);
    }

    public User? FindById(int userId, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            SELECT UserId, UserCode, FullName, BirthDate, Gender, PhoneNumber, Email, IsActive
            FROM Users
            WHERE UserId = @UserId
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@UserId", userId);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return MapUser(reader);
    }

    public User? FindByCode(string userCode)
    {
        using var conn = Db.CreateConnection();
        conn.Open();

        const string sql = """
            SELECT UserId, UserCode, FullName, BirthDate, Gender, PhoneNumber, Email, IsActive
            FROM Users
            WHERE UserCode = @UserCode
            """;

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UserCode", userCode);
        using var reader = cmd.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return MapUser(reader);
    }

    public void Add(User user)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        Add(user, conn, null);
    }

    public void Add(User user, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            INSERT INTO Users (UserCode, FullName, BirthDate, Gender, PhoneNumber, Email, IsActive)
            VALUES (@UserCode, @FullName, @BirthDate, @Gender, @PhoneNumber, @Email, @IsActive)
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@UserCode", user.UserCode);
        cmd.Parameters.AddWithValue("@FullName", user.FullName);
        cmd.Parameters.AddWithValue("@BirthDate", user.BirthDate);
        cmd.Parameters.AddWithValue("@Gender", user.Gender);
        cmd.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
        cmd.ExecuteNonQuery();
    }

    public void Update(User user)
    {
        using var conn = Db.CreateConnection();
        conn.Open();
        Update(user, conn, null);
    }

    public void Update(User user, SqlConnection connection, SqlTransaction? transaction)
    {
        const string sql = """
            UPDATE Users
            SET UserCode = @UserCode,
                FullName = @FullName,
                BirthDate = @BirthDate,
                Gender = @Gender,
                PhoneNumber = @PhoneNumber,
                Email = @Email,
                IsActive = @IsActive
            WHERE UserId = @UserId
            """;

        using var cmd = new SqlCommand(sql, connection, transaction);
        cmd.Parameters.AddWithValue("@UserId", user.UserId);
        cmd.Parameters.AddWithValue("@UserCode", user.UserCode);
        cmd.Parameters.AddWithValue("@FullName", user.FullName);
        cmd.Parameters.AddWithValue("@BirthDate", user.BirthDate);
        cmd.Parameters.AddWithValue("@Gender", user.Gender);
        cmd.Parameters.AddWithValue("@PhoneNumber", (object?)user.PhoneNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
        cmd.ExecuteNonQuery();
    }

    private static User MapUser(SqlDataReader reader)
    {
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
}
