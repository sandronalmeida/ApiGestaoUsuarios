using ApiGestaoUsuarios.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ApiGestaoUsuarios.Services
{
    public class PasswordService : IPasswordService
    {
        private readonly IPasswordHasher<string> _hasher;

        public PasswordService(IPasswordHasher<string> hasher)
        {
            _hasher = hasher;
        }
        public string Hash(string username, string password)
        {
            return _hasher.HashPassword(username, password);
        }

        public bool Verify(string username, string storedHash, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(username, storedHash, providedPassword);

            return result != PasswordVerificationResult.Failed;
        }
    }
}