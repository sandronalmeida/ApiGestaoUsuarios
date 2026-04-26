using ApiGestaoUsuarios.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace ApiGestaoUsuarios.Infraestructure.Data
{
    public class ApiGestaoUsuariosDbContext : DbContext
    {
        public ApiGestaoUsuariosDbContext(DbContextOptions<ApiGestaoUsuariosDbContext> options) : base(options)
        {
        }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração da entidade Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                // Chave primária
                entity.HasKey(u => u.Id);

                // Campos obrigatórios
                entity.Property(u => u.Nome)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(u => u.SenhaHash)
                      .IsRequired();

                // Valores padrão
                entity.Property(u => u.Ativo)
                      .HasDefaultValue(true);

                entity.Property(u => u.CriadoEm)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Campos opcionais
                entity.Property(u => u.AtualizadoEm);
                entity.Property(u => u.Cargo)
                      .HasMaxLength(100);
            });

        }
    }
}
