using CashFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace CashFlow.Api.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Mapeando as classes para virarem tabelas no banco de dados
        public DbSet<Pessoa> Pessoas { get; set; }
        public DbSet<Transacao> Transacoes { get; set; }
    }
}