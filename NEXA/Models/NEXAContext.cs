using Microsoft.EntityFrameworkCore;
using NEXA.Models;



namespace NEXA.Models
{
    public class NEXAContext:DbContext
    {
        public NEXAContext(DbContextOptions<NEXAContext> options) : base(options) { }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Inventario> Inventario { get; set; }

    }
}
