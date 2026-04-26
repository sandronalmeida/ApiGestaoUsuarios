namespace ApiGestaoUsuarios.Dtos
{
    public class UsuarioUpdateDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public string? Cargo { get; set; }
    }
}
