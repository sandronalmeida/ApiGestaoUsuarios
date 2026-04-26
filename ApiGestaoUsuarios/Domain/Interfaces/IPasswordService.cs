namespace ApiGestaoUsuarios.Domain.Interfaces
{
    public interface IPasswordService
    {
        string Hash(string username, string password);
        bool Verify(string username, string storedHash, string providedPassword);
    }
}
