namespace ApiGestaoUsuarios.Domain.Entities
{
    /// <summary>
    /// Representa a entidade de usuário no sistema.
    /// </summary>
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string SenhaHash { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
        public string? Cargo { get; set; }
    }
}
