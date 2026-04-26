using ApiGestaoUsuarios.Domain.Entities;
using ApiGestaoUsuarios.Dtos;

namespace ApiGestaoUsuarios.Domain.Interfaces
{
    /// <summary>
    /// Interface para abstração das regras de negócio relacionadas à gestão de usuários.
    /// Define operações de consulta, criação, atualização, desativação e geração de dados fictícios.
    /// </summary>
    public interface IUsuarioService
    {
        /// <summary>
        /// Lista todos os usuários cadastrados no sistema, independentemente de estarem ativos ou não.
        /// </summary>
        /// <returns>Uma coleção de objetos <see cref="Usuario"/>.</returns>
        Task<IEnumerable<UsuarioReadDto>> ListarTodosUsuariosAsync();

        /// <summary>
        /// Lista apenas os usuários que estão ativos no sistema.
        /// </summary>
        /// <returns>Uma coleção de usuários ativos.</returns>
        Task<IEnumerable<UsuarioReadDto>> ListarTodosUsuariosAtivosAsync();
        /// <summary>
        /// Busca um usuário específico pelo seu identificador único (GUID).
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <returns>
        /// O objeto <see cref="Usuario"/> correspondente, ou <c>null</c> se não encontrado.
        /// </returns>
        Task<UsuarioReadDto?> BuscarUsuarioPorIdAsync(Guid id);
        /// <summary>
        /// Busca um usuário específico pelo seu endereço de e-mail.
        /// </summary>
        /// <param name="email">E-mail do usuário.</param>
        /// <returns>
        /// O objeto <see cref="Usuario"/> correspondente, ou <c>null</c> se não encontrado.
        /// </returns>
        Task<UsuarioReadDto?> BuscarUsuarioPorEmailAsync(string email);
        /// <summary>
        /// Cria um novo usuário com base nos dados fornecidos.
        /// </summary>
        /// <param name="request">Objeto <see cref="UsuarioRequestDTO"/> contendo os dados do usuário.</param>
        /// <returns>O objeto <see cref="Usuario"/> recém-criado.</returns>
        Task<UsuarioReadDto> CriarUsuarioAsync(UsuarioRequestDto request);
        /// <summary>
        /// Atualiza os dados de um usuário existente.
        /// </summary>
        /// <param name="id">Identificador único do usuário a ser atualizado.</param>
        /// <param name="request">Objeto <see cref="UsuarioRequestDTO"/> com os novos dados.</param>
        /// <returns>
        /// <c>true</c> se a atualização foi bem-sucedida,  
        /// <c>false</c> se o usuário não foi encontrado ou não pôde ser atualizado.
        /// </returns>
        Task<bool> AtualizarUsuarioAsync(Guid id, UsuarioRequestDto request);
        /// <summary>
        /// Desativa um usuário existente, tornando-o inativo no sistema.
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <returns>
        /// <c>true</c> se o usuário foi desativado com sucesso,  
        /// <c>false</c> se o usuário não foi encontrado.
        /// </returns>
        Task<bool> DesativarUsuarioAsync(Guid id);
        /// <summary>
        /// Gera uma lista de usuários fictícios para testes e validações.
        /// </summary>
        /// <param name="quantidade">Quantidade de usuários a serem gerados.</param>
        /// <returns>Lista de objetos <see cref="Usuario"/> simulados.</returns>
        Task<List<Usuario>> GerarUsuariosFakeAsync(int quantidade);
        Task<(bool Success, string? ErrorMessage)> ExecutarOperacaoAsync(UsuarioRequestValidarDto dto);
        Task ApagarTodosUsuariosAsync();
    }
}
