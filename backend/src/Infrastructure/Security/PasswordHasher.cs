using Isopoh.Cryptography.Argon2;

namespace Quitly.Api.Infrastructure.Security;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);
}

public sealed class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return Argon2.Hash(password);
    }

    public bool Verify(string password, string hash)
    {
        return Argon2.Verify(hash, password);
    }
}
