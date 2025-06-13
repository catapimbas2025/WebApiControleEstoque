using Microsoft.EntityFrameworkCore;
using WebApiControleEstoque.Models;

namespace WebApiControleEstoque.Data
{
    public class AppDbContext : DbContext
    {
     public AppDbContext(DbContextOptions<AppDbContext>options) : base(options) { }

        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<Movimentacao> Movimentacoes => Set<Movimentacao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Produto>()
            .HasIndex(p => p.CodigoBarras)
            .IsUnique();

            modelBuilder.Entity<Movimentacao>()
            .HasOne(m => m.Produto)
            .WithMany()
            .HasForeignKey(m => m.ProdutoId)
            .OnDelete(DeleteBehavior.Cascade);

        }


    }
}
