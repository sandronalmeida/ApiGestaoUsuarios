using ApiGestaoUsuarios.Domain.Entities;
using ApiGestaoUsuarios.Domain.Interfaces;
using ApiGestaoUsuarios.Dtos;
using ApiGestaoUsuarios.Infraestructure.Data;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace ApiGestaoUsuarios.Services
{
    /// <summary>
    /// Serviço responsável pela gestão de usuários no sistema.
    /// Implementa operações de CRUD, autenticação e geração de dados simulados.
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private readonly ApiGestaoUsuariosDbContext _context;
        private readonly IPasswordService _passwordHasher;

        public UsuarioService(ApiGestaoUsuariosDbContext context, IPasswordService PasswordService)
        {
            _context = context;
            _passwordHasher = PasswordService;
        }

        /// <summary>
        /// Lista todos os usuários cadastrados no sistema.
        /// </summary>
        /// <returns>Lista de objetos <see cref="UsuarioReadDto"/>.</returns>
        public async Task<IEnumerable<UsuarioReadDto>> ListarTodosUsuariosAsync()
        {
            return await _context.Usuarios
                .Select(u => new UsuarioReadDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Ativo = u.Ativo,
                    CriadoEm = u.CriadoEm,
                    AtualizadoEm = u.AtualizadoEm,
                    Cargo = u.Cargo
                })
                .ToListAsync();
        }

        /// <summary>
        /// Lista apenas os usuários ativos no sistema.
        /// </summary>
        /// <returns>Lista de objetos <see cref="UsuarioReadDto"/> correspondentes a usuários ativos.</returns>
        public async Task<IEnumerable<UsuarioReadDto>> ListarTodosUsuariosAtivosAsync()
        {
            return await _context.Usuarios
                .Where(u => u.Ativo)
                .Select(u => new UsuarioReadDto
                {
                    Id = u.Id,
                    Nome = u.Nome,
                    Email = u.Email,
                    Ativo = u.Ativo,
                    CriadoEm = u.CriadoEm,
                    AtualizadoEm = u.AtualizadoEm,
                    Cargo = u.Cargo
                })
                .ToListAsync();
        }

        /// <summary>
        /// Busca um usuário específico pelo seu identificador único (GUID).
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <returns>Objeto <see cref="UsuarioReadDto"/> se encontrado; caso contrário, null.</returns>
        public async Task<UsuarioReadDto?> BuscarUsuarioPorIdAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return null;

            return new UsuarioReadDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Ativo = usuario.Ativo,
                CriadoEm = usuario.CriadoEm,
                AtualizadoEm = usuario.AtualizadoEm,
                Cargo = usuario.Cargo
            };
        }

        /// <summary>
        /// Busca um usuário específico pelo seu endereço de e-mail.
        /// </summary>
        /// <param name="email">E-mail do usuário.</param>
        /// <returns>Objeto <see cref="UsuarioReadDto"/> se encontrado; caso contrário, null.</returns>
        public async Task<UsuarioReadDto?> BuscarUsuarioPorEmailAsync(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
            if (usuario == null) return null;

            return new UsuarioReadDto
            {
                Id = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email,
                Ativo = usuario.Ativo,
                CriadoEm = usuario.CriadoEm,
                AtualizadoEm = usuario.AtualizadoEm,
                Cargo = usuario.Cargo
            };
        }

        /// <summary>
        /// Cria um novo usuário com base nos dados fornecidos.
        /// </summary>
        /// <param name="request">Objeto contendo os dados do usuário a ser criado.</param>
        /// <returns>Objeto <see cref="UsuarioReadDto"/> representando o usuário criado.</returns>
        /// <exception cref="Exception">Lançada se o e-mail já estiver cadastrado.</exception>
        public async Task<UsuarioReadDto> CriarUsuarioAsync(UsuarioRequestDto request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                throw new Exception("Email já cadastrado.");

            var senhaHash = _passwordHasher.Hash(request.Nome,request.Senha);
            var novoUsuario = new Usuario
            {
                Nome = request.Nome,
                Email = request.Email,
                SenhaHash = senhaHash,
                Cargo = request.Cargo,
                CriadoEm = DateTime.UtcNow,
                Ativo = true
            };

            _context.Usuarios.Add(novoUsuario);
            await _context.SaveChangesAsync();

            return new UsuarioReadDto
            {
                Id = novoUsuario.Id,
                Nome = novoUsuario.Nome,
                Email = novoUsuario.Email,
                Ativo = novoUsuario.Ativo,
                CriadoEm = novoUsuario.CriadoEm,
                Cargo = novoUsuario.Cargo
            };
        }

        /// <summary>
        /// Atualiza os dados de um usuário existente.
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <param name="request">Objeto contendo os novos dados do usuário.</param>
        /// <returns>true se atualizado com sucesso; false se não encontrado.</returns>
        public async Task<bool> AtualizarUsuarioAsync(Guid id, UsuarioRequestDto request)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;
            
            if (!string.IsNullOrWhiteSpace(request.Nome))
                usuario.Nome = request.Nome;

            if (!string.IsNullOrWhiteSpace(request.Email))
                usuario.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Senha))
            {
                var senhaHash = _passwordHasher.Hash(request.Nome, request.Senha);
                usuario.SenhaHash = senhaHash;
            }
            if (!string.IsNullOrWhiteSpace(request.Cargo))
                usuario.Cargo = request.Cargo;

            usuario.AtualizadoEm = DateTime.UtcNow;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Desativa um usuário existente com base em seu identificador único.
        /// </summary>
        /// <param name="id">Identificador único do usuário.</param>
        /// <returns>true se desativado com sucesso; false se não encontrado.</returns>
        public async Task<bool> DesativarUsuarioAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            usuario.Ativo = false;
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Gera uma lista de usuários fictícios para testes e validações.
        /// </summary>
        /// <param name="quantidade">Quantidade de usuários a serem gerados (padrão: 20).</param>
        /// <returns>Lista de objetos <see cref="Usuario"/> simulados.</returns>
        public async Task<List<Usuario>> GerarUsuariosFakeAsync(int quantidade = 20)
        {
            var usuarios = new List<Usuario>();
            var faker = new Faker("pt_BR"); // gera dados em português
            int count = 0;

            while (count < quantidade)
            {
                var nomeFake = faker.Name.FullName();
                var emailFake = faker.Internet.Email(nomeFake);
                // Verifica se já existe usuário com mesmo nome ou email
                var existe = await _context.Usuarios
                    .AnyAsync(u => u.Nome == nomeFake || u.Email == emailFake);

                if (existe)
                {
                    // Se já existe, pula para o próximo loop
                    continue;
                }
                var senhaHash = _passwordHasher.Hash(nomeFake, $"Senha{count+1}123");
                var usuario = new Usuario
                {
                    Nome = nomeFake,
                    Email = emailFake,
                    SenhaHash = senhaHash,
                    Cargo = faker.PickRandom(new[] { "Administrador", "Colaborador", "Gerente", "Analista","Atendente","Técnico" }),
                    CriadoEm = DateTime.UtcNow,
                    Ativo = true
                };

                usuarios.Add(usuario);
                _context.Usuarios.Add(usuario);
                count++;
            }

            await _context.SaveChangesAsync();
            return usuarios;
        }

        /// <summary>
        /// Executa uma operação autenticada, validando usuário e senha.
        /// </summary>
        /// <param name="dto">Objeto contendo dados de autenticação.</param>
        /// <returns>
        /// Tupla indicando sucesso ou falha, com mensagem de erro em caso de falha.
        /// </returns>
        public async Task<(bool Success, string? ErrorMessage)> ExecutarOperacaoAsync(UsuarioRequestValidarDto dto)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Email.Equals(dto.Email));

            if (usuario is null)
            {
                return (false, "Usuário não encontrado");
            }

            var senhaValida = _passwordHasher.Verify(usuario.Nome, usuario.SenhaHash, dto.Senha);
            if (!senhaValida)
            {
                return (false, "Senha inválida");
            }                    
            return (true, null);
        }

        /// <summary>
        /// Apaga todos os usuários do banco de dados.
        /// Atenção: esta operação é destrutiva e irreversível.
        /// </summary>
        public async Task ApagarTodosUsuariosAsync()
        {
            var todosUsuarios = _context.Usuarios.ToList();

            if (todosUsuarios.Any())
            {
                _context.Usuarios.RemoveRange(todosUsuarios);
                await _context.SaveChangesAsync();
            }
        }
    }
}
