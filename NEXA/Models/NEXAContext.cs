using Microsoft.EntityFrameworkCore;
using NEXA.Models;



namespace NEXA.Models
{
    public class NEXAContext:DbContext
    {
        public NEXAContext(DbContextOptions<NEXAContext> options) : base(options) { }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }


        public DbSet<Proyecto> Proyecto { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relación uno a muchos entre Usuario y Proyecto
            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Proyectos)
                .HasForeignKey(p => p.UsuarioID)
                .OnDelete(DeleteBehavior.Cascade); // Opcional: eliminar proyectos si se elimina el usuario
        }

    }
}
