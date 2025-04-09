using Microsoft.EntityFrameworkCore;
using AppPromocoesGamer.API.Models;


namespace AppPromocoesGamer.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Promocao> Promocoes { get; set; }
        public DbSet<Comentario> Comentarios { get; set; }


    }
}
