using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Quitly.Api.Infrastructure.Security;

namespace Quitly.IntegrationTests;

/// <summary>
/// T056: Encryption migration test.
///
/// Verifies that FieldEncryptor.Encrypt → Decrypt round-trips correctly:
///   - plaintext → Encrypt → Decrypt → same plaintext (lossless)
///   - Encrypted bytes differ from the plaintext bytes (not stored as plain)
///   - Cross-user isolation: ciphertext from user A cannot be decrypted by user B
///
/// Uses Microsoft.AspNetCore.DataProtection with an ephemeral key provider
/// (no key persistence required) so tests run without file system access.
/// </summary>
public sealed class EncryptionMigrationTests
{
    private static FieldEncryptor CreateEncryptor()
    {
        var provider = new EphemeralDataProtectionProvider();
        return new FieldEncryptor(provider);
    }

    [Fact]
    public void Encrypt_ThenDecrypt_ReturnsOriginalPlaintext()
    {
        var encryptor = CreateEncryptor();
        var userId = Guid.NewGuid();
        const string plaintext = "Stress bei der Arbeit heute Abend.";

        var ciphertext = encryptor.Encrypt(plaintext, userId);
        var decrypted = encryptor.Decrypt(ciphertext, userId);

        decrypted.Should().Be(plaintext);
    }

    [Fact]
    public void Encrypt_ProducesBytesNotEqualToPlaintextBytes()
    {
        var encryptor = CreateEncryptor();
        var userId = Guid.NewGuid();
        const string plaintext = "Some context note";

        var ciphertext = encryptor.Encrypt(plaintext, userId);
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);

        ciphertext.Should().NotEqual(plaintextBytes, because: "ciphertext must differ from plaintext bytes");
    }

    [Fact]
    public void Decrypt_WithDifferentUserId_Throws()
    {
        var encryptor = CreateEncryptor();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        const string plaintext = "Private note";

        var ciphertext = encryptor.Encrypt(plaintext, userId);

        var act = () => encryptor.Decrypt(ciphertext, otherUserId);
        act.Should().Throw<Exception>(because: "ciphertext from user A cannot be decrypted with user B's key");
    }

    [Fact]
    public void Encrypt_EmptyString_RoundTrips()
    {
        var encryptor = CreateEncryptor();
        var userId = Guid.NewGuid();

        var ciphertext = encryptor.Encrypt(string.Empty, userId);
        var decrypted = encryptor.Decrypt(ciphertext, userId);

        decrypted.Should().BeEmpty();
    }

    [Fact]
    public void Encrypt_500CharNote_RoundTrips()
    {
        var encryptor = CreateEncryptor();
        var userId = Guid.NewGuid();
        var plaintext = new string('A', 500);

        var ciphertext = encryptor.Encrypt(plaintext, userId);
        var decrypted = encryptor.Decrypt(ciphertext, userId);

        decrypted.Should().Be(plaintext);
    }
}
