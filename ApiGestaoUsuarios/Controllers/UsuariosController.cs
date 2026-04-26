using ApiGestaoUsuarios.Domain.Entities;
using ApiGestaoUsuarios.Domain.Interfaces;
using ApiGestaoUsuarios.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiGestaoUsuarios.Controllers
{    
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
        /// Lista apenas os usuários ativos.
        /// </summary>
        /// <returns>Lista de usuários ativos.</returns>
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
        /// <returns>
        /// 200 - Usuário encontrado.  
        /// 404 - Usuário não encontrado.
        /// </returns>
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
        /// <param name="email">E-mail do usuário.</param>
        /// <returns>
        /// 200 - Usuário encontrado.  
        /// 404 - Usuário não encontrado.
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
        /// Cria um novo usuário com base nos dados fornecidos.
        /// </summary>
        /// <param name="request">Objeto contendo os dados do usuário a ser criado.</param>
        /// <returns>
        /// 201 - Usuário criado com sucesso (retorna a rota para consulta por ID).  
        /// 400 - Dados inválidos.
        /// </returns>
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
        /// Atualiza os dados de um usuário existente.
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <param name="request">Objeto contendo os novos dados do usuário.</param>
        /// <returns>
        /// 200 - Usuário atualizado com sucesso.  
        /// 404 - Usuário não encontrado.  
        /// 400 - Dados inválidos.
        /// </returns>
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
        /// Desativa um usuário existente com base em seu identificador único.
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <returns>
        /// 204 - Usuário desativado com sucesso.  
        /// 404 - Usuário não encontrado.
        /// </returns>
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
        /// Gera uma lista de usuários fictícios para testes e validações.
        /// </summary>
        /// <param name="quantidade">Quantidade de usuários a serem gerados (padrão: 20).</param>
        /// <returns>Lista de usuários simulados.</returns>
        [HttpGet("UsuariosFake")]
        public async Task<ActionResult<List<Usuario>>> GetUsuariosFake([FromQuery] int quantidade = 20)
        {
            var usuarios = await _service.GerarUsuariosFakeAsync(quantidade);
            return Ok(usuarios);
        }
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
        /// Apaga todos os usuários do banco de dados.
        /// Atenção: esta operação é destrutiva e irreversível.
        /// </summary>
        [HttpDelete("apagar-todos")]
        public async Task<IActionResult> ApagarTodosUsuarios()
        {
            await _service.ApagarTodosUsuariosAsync();
            return NoContent(); // 204 - Operação concluída sem retorno
        }

    }
}
