namespace ApiGestaoUsuarios.Dtos
{
    public class UsuarioRequestDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        // A senha vem em texto puro e será convertida para hash no Service
        public string Senha { get; set; } = string.Empty;
        public string? Cargo { get; set; }
    }
}
