using System.Security.Cryptography;
using HomeWidgets.Core.Interfaces;

namespace HomeWidgets.Infrastructure.Authentication;

/// <summary>
/// Password hasher using PBKDF2 with SHA256.
/// Industry standard for secure password storage (OWASP recommended).
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;      // 128 bits - random data added to password
    private const int KeySize = 32;       // 256 bits - output hash size
    private const int Iterations = 100000; // OWASP recommended minimum
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    private const char Delimiter = ':';

    /// <summary>
    /// Hashes a password using PBKDF2.
    /// Returns format: "salt:hash" (both Base64 encoded).
    /// Each call generates a unique salt, so same password = different hash.
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        // Generate unique random salt for this password
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Hash password with salt using PBKDF2
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize);

        // Store as "salt:hash" so we can retrieve salt for verification
        return $"{Convert.ToBase64String(salt)}{Delimiter}{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verifies a password against a stored hash.
    /// Extracts salt from stored hash, re-hashes input, compares.
    /// Uses constant-time comparison to prevent timing attacks.
    /// </summary>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;

        if (string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        // Split stored "salt:hash" to get original salt
        var parts = hashedPassword.Split(Delimiter);
        if (parts.Length != 2)
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[0]);
            var storedHash = Convert.FromBase64String(parts[1]);

            // Hash input password with SAME salt
            var inputHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize);

            // Constant-time comparison prevents timing attacks
            return CryptographicOperations.FixedTimeEquals(inputHash, storedHash);
        }
        catch
        {
            return false;
        }
    }
}

