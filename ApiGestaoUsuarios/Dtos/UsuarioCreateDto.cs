namespace ApiGestaoUsuarios.Dtos
{      
    public class UsuarioCreateDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty; // senha em texto, será convertida para hash
        public string? Cargo { get; set; }
    }
   
}
