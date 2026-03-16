using Microsoft.Data.SqlClient;

namespace LibrarySystem.Common;

public static class Db
{
    // 大会版: 接続文字列はここで一元管理する
    private const string ConnectionString =
        "Server=localhost;Database=LibrarySystem;Trusted_Connection=True;TrustServerCertificate=True;";

    public static SqlConnection CreateConnection()
    {
        return new SqlConnection(ConnectionString);
    }
}
