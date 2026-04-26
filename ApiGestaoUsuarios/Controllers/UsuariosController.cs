using ApiGestaoUsuarios.Domain.Entities;
using ApiGestaoUsuarios.Domain.Interfaces;
using ApiGestaoUsuarios.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiGestaoUsuarios.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão de usuários e exposição de metadados da API.
    /// </summary>
    [ApiController]
    [Route("api/v1/GestaoUsuarios")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _service;
        private readonly EndpointDataSource _endpointDataSource;
        public UsuariosController(IUsuarioService service, EndpointDataSource endpointDataSource)
        {
            _service = service;
            _endpointDataSource = endpointDataSource;
        }

        /// <summary>
        /// Lista todos os endpoints registrados na aplicação com detalhes técnicos.
        /// </summary>
        /// <remarks>
        /// Útil para auditoria de rotas, verificando parâmetros, métodos HTTP e filtros aplicados.
        /// </remarks>
        /// <returns>Uma coleção de objetos contendo metadados das rotas.</returns>
        /// <response code="200">Retorna a lista de endpoints com sucesso.</response>
        [HttpGet("endpoints")]
        public IActionResult GetAllEndpoints()
        {
            var endpoints = _endpointDataSource.Endpoints
                .OfType<RouteEndpoint>()
                .Select(e =>
                {
                    var actionDescriptor = e.Metadata
                        .OfType<ControllerActionDescriptor>()
                        .FirstOrDefault();

                    return new
                    {                       
                        Parâmetros = e.RoutePattern.Parameters
                            .Select(p => new { p.Name, p.ParameterPolicies })
                            .ToArray(),
                        Rota = e.RoutePattern.RawText,
                        Métodos = e.Metadata
                            .OfType<HttpMethodMetadata>()
                            .FirstOrDefault()?.HttpMethods,
                        Controller = actionDescriptor?.ControllerName,
                        Action = actionDescriptor?.ActionName,
                        Retorno = actionDescriptor?.MethodInfo.ReturnType.Name,
                        Filtros = e.Metadata
                            .OfType<IFilterMetadata>()
                            .Select(f => f.GetType().Name)
                            .ToArray()
                    };
                });

            return Ok(endpoints);
        }

        /// <summary>
        /// Lista todos os usuários cadastrados no sistema.
        /// </summary>
        /// <returns>Lista de objetos <see cref="Usuario"/>.</returns>
        /// <response code="200">Retorna a lista de usuários.</response>
        /// <response code="404">Nenhum usuário encontrado na base de dados.</response>
        [HttpGet("usuarios")]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _service.ListarTodosUsuariosAsync();

            if (usuarios == null || !usuarios.Any())
            {
                return NotFound(new { mensagem = "Nenhum usuário encontrado." });
            }

            return Ok(usuarios);
        }

        /// <summary>
        /// Lista apenas os usuários que possuem o status ativo.
        /// </summary>
        /// <returns>Lista de usuários ativos.</returns>
        /// <response code="200">Retorna a lista de usuários ativos.</response>
        /// <response code="404">Nenhum usuário ativo encontrado.</response>
        [HttpGet("UsuariosAtivos")]
        public async Task<IActionResult> GetAllAtivos()
        {
            var usuarios = await _service.ListarTodosUsuariosAtivosAsync();

            if (usuarios == null || !usuarios.Any())
            {
                return NotFound(new { mensagem = "Nenhum usuário encontrado." });
            }

            return Ok(usuarios);

        }

        /// <summary>
        /// Busca um usuário específico pelo seu identificador único (GUID).
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <returns>Os dados do usuário solicitado.</returns>
        /// <response code="200">Usuário encontrado com sucesso.</response>
        /// <response code="400">O ID fornecido é inválido ou vazio.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpGet("Usuario/{id:guid}", Name = "ObterUsuarioPorId")]
        public async Task<IActionResult> GetById(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "O parâmetro 'id' é inválido." });
            }
            var user = await _service.BuscarUsuarioPorIdAsync(id);
            return user == null ? NotFound(new { message = "Usuário não encontrado." }) : Ok(user);
        }

        /// <summary>
        /// Busca um usuário específico pelo seu endereço de e-mail.
        /// </summary>
        /// <param name="email">Endereço de e-mail cadastrado.</param>
        /// <returns>Os dados do usuário solicitado.</returns>
        /// <response code="200">Usuário encontrado com sucesso.</response>
        /// <response code="400">E-mail em formato inválido ou vazio.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpGet("Usuario/{email}", Name = "ObterUsuarioPorEmail")]        
        public async Task<IActionResult> GetByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest(new { message = "O parâmetro 'email' é inválido." });
            }

            // Opcional: validar formato de email
            try
            {
                var enderecoEmail = new System.Net.Mail.MailAddress(email);
                if (enderecoEmail.Address != email)
                {
                    return BadRequest(new { message = "Formato de email inválido." });
                }
            }
            catch
            {
                return BadRequest(new { message = "Formato de email inválido." });
            }

            var user = await _service.BuscarUsuarioPorEmailAsync(email);
            return user == null ? NotFound(new { message = "Usuário não encontrado." }) : Ok(user);
        }

        /// <summary>
        /// Cria um novo usuário no sistema.
        /// </summary>
        /// <param name="request">Dados necessários para a criação do usuário.</param>
        /// <returns>O usuário recém-criado e o link para consulta.</returns>
        /// <response code="201">Usuário criado com sucesso.</response>
        /// <response code="400">Dados da requisição inválidos ou campos obrigatórios ausentes.</response>
        /// <response code="500">Erro interno ao processar a criação.</response>
        [HttpPost("usuario")]
        //[ServiceFilter(typeof(ValidationFilter))]
        // filtro global adicionado no program.cs
        public async Task<IActionResult> Create([FromBody] UsuarioRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Requisição inválida." });
            }

            if (string.IsNullOrWhiteSpace(request.Nome) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "Nome e email são obrigatórios." });
            }
            try
            {
                var novo = await _service.CriarUsuarioAsync(request);

                return CreatedAtRoute(
                    "ObterUsuarioPorId",
                    new { id = novo.Id },
                    novo
                );
            }
            catch (Exception ex)
            {
                // Aqui você pode logar o erro com Serilog
                return StatusCode(500, new { message = "Erro ao criar usuário.", detalhe = ex.Message });
            }
        }

        /// <summary>
        /// Atualiza parcialmente os dados de um usuário existente.
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <param name="request">Novos dados do usuário.</param>
        /// <returns>Mensagem de confirmação da atualização.</returns>
        /// <response code="200">Usuário atualizado com sucesso.</response>
        /// <response code="400">ID inválido ou dados da requisição inconsistentes.</response>
        /// <response code="404">Usuário não encontrado para atualização.</response>
        [HttpPatch("Usuario/atualizar/{id:guid}")]
        //[ServiceFilter(typeof(ValidationFilter))]
        // filtro global adicionado no program.cs
        public async Task<IActionResult> Update(Guid id, [FromBody] UsuarioRequestDto request)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "O parâmetro 'id' é inválido." });
            }

            if (request == null)
            {
                return BadRequest(new { message = "Requisição inválida." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var sucesso = await _service.AtualizarUsuarioAsync(id, request);

            if (!sucesso)
                return NotFound(new { message = "Usuário não encontrado." });

            return Ok(new { message = "Usuário atualizado com sucesso." });
        }

        /// <summary>
        /// Desativa um usuário no sistema (Exclusão lógica).
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        /// <response code="204">Usuário desativado com sucesso.</response>
        /// <response code="400">ID fornecido é inválido.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpPatch("Usuario/desativar/{id:guid}")]
        public async Task<IActionResult> Disable(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest(new { message = "O parâmetro 'id' é inválido." });
            }

            var sucesso = await _service.DesativarUsuarioAsync(id);
            return sucesso ? NoContent() : NotFound();
        }

        /// <summary>
        /// Gera uma lista de usuários fictícios para fins de teste e preenchimento de base.
        /// </summary>
        /// <param name="quantidade">Número de usuários a serem gerados (padrão é 20).</param>
        /// <returns>Uma lista de usuários simulados.</returns>
        /// <response code="200">Lista gerada com sucesso.</response>
        [HttpGet("UsuariosFake")]
        public async Task<ActionResult<List<Usuario>>> GetUsuariosFake([FromQuery] int quantidade = 20)
        {
            var usuarios = await _service.GerarUsuariosFakeAsync(quantidade);
            return Ok(usuarios);
        }

        /// <summary>
        /// Executa uma operação que exige validação de credenciais do usuário.
        /// </summary>
        /// <param name="dto">Objeto contendo e-mail e senha para validação.</param>
        /// <returns>Resultado da autenticação.</returns>
        /// <response code="200">Usuário autenticado e operação permitida.</response>
        /// <response code="401">Senha inválida ou não autorizada.</response>
        /// <response code="404">Usuário correspondente ao e-mail não encontrado.</response>
        /// <response code="500">Erro interno na execução da operação.</response>
        [HttpPost("operacao-autenticada")]
        //[ServiceFilter(typeof(ValidationFilter))]
        // filtro global adicionado no program.cs
        public async Task<IActionResult> Operacao([FromBody] UsuarioRequestValidarDto dto)
        {
            if (dto == null)
            {
                return BadRequest(new { mensagem = "Requisição inválida." });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var resultado = await _service.ExecutarOperacaoAsync(dto);

            if (!resultado.Success)
            {
                if (resultado.ErrorMessage == "Usuário não encontrado")
                    return NotFound(new { mensagem = resultado.ErrorMessage });

                if (resultado.ErrorMessage == "Senha inválida")
                    return Unauthorized(new { mensagem = resultado.ErrorMessage });

                // fallback para outros erros
                return StatusCode(500, new { mensagem = "Erro interno na operação." });
            }

            return Ok(new { mensagem = "Usuário autenticado com sucesso." });
        }

        /// <summary>
        /// Remove permanentemente todos os usuários da base de dados.
        /// </summary>
        /// <remarks>
        /// Atenção: Esta operação é destrutiva, irreversível e apagará todos os registros.
        /// </remarks>
        /// <response code="204">Operação realizada com sucesso (base limpa).</response>
        [HttpDelete("apagar-todos")]
        public async Task<IActionResult> ApagarTodosUsuarios()
        {
            await _service.ApagarTodosUsuariosAsync();
            return NoContent(); // 204 - Operação concluída sem retorno
        }

    }
}
