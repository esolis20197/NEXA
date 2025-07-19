using Microsoft.EntityFrameworkCore;

using NEXA.Models;

namespace NEXA.Models
{
    public class NEXAContext : DbContext
    {
        public NEXAContext(DbContextOptions<NEXAContext> options) : base(options) { }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Inventario> Inventario { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<CarritoDetalle> CarritoDetalles { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<Proyecto> Proyecto { get; set; }
        public DbSet<Citas> Citas { get; set; }

        public DbSet<PermisoInstalacion> PermisosInstalacion { get; set; }

        public DbSet<CorreosPromocional> CorreosPromocionales { get; set; }
        public DbSet<GrupoCliente> GrupoClientes { get; set; }
        public DbSet<ClientesPorGrupo> ClientesPorGrupos { get; set; }

        public DbSet<ReporteHistorial> ReporteHistoriales { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relación uno a muchos entre Usuario y Proyecto
            modelBuilder.Entity<Proyecto>()
                .HasOne(p => p.Usuario)
                .WithMany(u => u.Proyectos)
                .HasForeignKey(p => p.UsuarioID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relación uno a muchos entre Usuario y Citas
            modelBuilder.Entity<Citas>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Citas)
                .HasForeignKey(c => c.UsuarioID)
                .OnDelete(DeleteBehavior.Cascade);

            //Validacion citas repetidas
            modelBuilder.Entity<Citas>()
                 .HasIndex(c => new { c.UsuarioID, c.FechaCita })
                 .IsUnique();

            // Relación uno a muchos entre Proyecto y PermisosInstalacion
            modelBuilder.Entity<PermisoInstalacion>()
                .HasOne(p => p.Proyecto)
                .WithMany(pr => pr.PermisosInstalacion)
                .HasForeignKey(p => p.ProyectoID)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación CorreosPromocional -> Usuario (muchos a uno)
            modelBuilder.Entity<CorreosPromocional>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.CorreosPromocionales)  
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict); 

            // Relación CorreosPromocional -> GrupoCliente (muchos a uno)
            modelBuilder.Entity<CorreosPromocional>()
                .HasOne(c => c.GrupoCliente)
                .WithMany(g => g.CorreosPromocionales)
                .HasForeignKey(c => c.GrupoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación ClientesPorGrupo -> GrupoCliente (muchos a uno)
            modelBuilder.Entity<ClientesPorGrupo>()
                .HasOne(cpg => cpg.GrupoCliente)
                .WithMany(g => g.ClientesPorGrupo)
                .HasForeignKey(cpg => cpg.GrupoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación ClientesPorGrupo -> Usuario (muchos a uno)
            modelBuilder.Entity<ClientesPorGrupo>()
                .HasOne(cpg => cpg.Usuario)
                .WithMany(u => u.ClientesPorGrupo)
                .HasForeignKey(cpg => cpg.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}