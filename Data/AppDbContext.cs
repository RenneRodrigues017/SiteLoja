using Microsoft.EntityFrameworkCore;
using SiteLoja.Models;

namespace SiteLoja.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions) { }
        
        public DbSet<Produto> ProdutosDb { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Endereco> Enderecos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<ItemPedido> ItensPedido { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Produto>()
                .Property(p => p.Preco)
                .HasPrecision(18, 2); // 18 dígitos no total, 2 após a vírgula (Ex: 1234567890123456.78)

            modelBuilder.Entity<Produto>()
                .Property(p => p.PrecoOriginal)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Usuario>()
    .HasOne(u => u.EnderecoCliente)
    .WithOne()
    .HasForeignKey<Usuario>(u => u.EnderecoClienteId)
    .IsRequired(false); // CRUCIAL que seja false

            modelBuilder.Entity<Endereco>(entity =>
            {
                entity.Property(e => e.Preco).HasPrecision(18, 2);
                entity.Property(e => e.PrecoPix).HasPrecision(18, 2);
                entity.Property(e => e.Frete).HasPrecision(18, 2);
                entity.Property(e => e.PrecoTotal).HasPrecision(18, 2);
            });


        }
    }
}