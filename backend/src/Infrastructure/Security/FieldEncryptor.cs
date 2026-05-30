using Microsoft.AspNetCore.DataProtection;

namespace Quitly.Api.Infrastructure.Security;

/// <summary>
/// Encrypts and decrypts short text fields using ASP.NET Core Data Protection,
/// scoped per-user so that cross-user decryption is impossible even if the DB is leaked.
///
/// Purpose string includes the userId, meaning a ciphertext produced for user A
/// cannot be decrypted by user B's key — Constitution I (User Welfare/Privacy).
/// </summary>
public sealed class FieldEncryptor(IDataProtectionProvider dataProtectionProvider)
{
    /// <summary>
    /// Encrypts <paramref name="plaintext"/> using a user-scoped data protector.
    /// Returns the raw protected bytes suitable for storage as PostgreSQL bytea.
    /// </summary>
    public byte[] Encrypt(string plaintext, Guid userId)
    {
        var protector = GetProtector(userId);
        return protector.Protect(System.Text.Encoding.UTF8.GetBytes(plaintext));
    }

    /// <summary>
    /// Decrypts bytes previously produced by <see cref="Encrypt"/> for the same user.
    /// Throws <see cref="System.Security.Cryptography.CryptographicException"/> on
    /// key mismatches or tampered ciphertexts — callers should handle and return null.
    /// </summary>
    public string Decrypt(byte[] ciphertext, Guid userId)
    {
        var protector = GetProtector(userId);
        return System.Text.Encoding.UTF8.GetString(protector.Unprotect(ciphertext));
    }

    private IDataProtector GetProtector(Guid userId)
        => dataProtectionProvider.CreateProtector($"Quitly.Relapse.ContextNote.{userId}");
}
