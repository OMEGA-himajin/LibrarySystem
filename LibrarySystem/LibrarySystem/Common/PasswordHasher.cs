using System.Security.Cryptography;

namespace LibrarySystem.Common;

public static class PasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 64;
    private const int Iterations = 100_000;

    public static string GenerateSalt()
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        return Convert.ToBase64String(salt);
    }

    public static string Hash(string password, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(
            password,
            saltBytes,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);
        return Convert.ToBase64String(hashBytes);
    }

    public static bool Verify(string password, string storedHash, string storedSalt)
    {
        var computedHash = Hash(password, storedSalt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedHash),
            Convert.FromBase64String(storedHash));
    }
}
