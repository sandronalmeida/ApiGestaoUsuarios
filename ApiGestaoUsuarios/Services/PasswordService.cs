using ApiGestaoUsuarios.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ApiGestaoUsuarios.Services
{
    /// <summary>
    /// Serviço responsável por operações de hashing e verificação de senhas.
    /// Utiliza a implementação de <see cref="IPasswordHasher{TUser}"/> do ASP.NET Identity.
    /// </summary>
    public class PasswordService : IPasswordService
    {
        /// <summary>
        /// Inicializa uma nova instância do <see cref="PasswordService"/>.
        /// </summary>
        /// <param name="hasher">Implementação de <see cref="IPasswordHasher{TUser}"/> usada para gerar e verificar hashes.</param>
        private readonly IPasswordHasher<string> _hasher;

        public PasswordService(IPasswordHasher<string> hasher)
        {
            _hasher = hasher;
        }
        /// <summary>
        /// Gera o hash de uma senha fornecida para um determinado usuário.
        /// </summary>
        /// <param name="username">Nome de usuário associado ao hash.</param>
        /// <param name="password">Senha em texto puro a ser convertida em hash.</param>
        /// <returns>String contendo o hash da senha.</returns>
        public string Hash(string username, string password)
        {
            return _hasher.HashPassword(username, password);
        }

        /// <summary>
        /// Verifica se a senha fornecida corresponde ao hash armazenado.
        /// </summary>
        /// <param name="username">Nome de usuário associado ao hash.</param>
        /// <param name="storedHash">Hash previamente armazenado da senha.</param>
        /// <param name="providedPassword">Senha em texto puro fornecida para validação.</param>
        /// <returns>
        /// true se a senha fornecida for válida;  
        /// false caso contrário.
        /// </returns>
        public bool Verify(string username, string storedHash, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(username, storedHash, providedPassword);

            return result != PasswordVerificationResult.Failed;
        }
    }
}