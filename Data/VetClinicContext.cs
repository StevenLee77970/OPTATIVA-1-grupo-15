using Microsoft.EntityFrameworkCore;
using VetClinicAPI.Models;

namespace VetClinicAPI.Data
{
    public class VetClinicContext : DbContext
    {
        public VetClinicContext(DbContextOptions<VetClinicContext> options) : base(options)
        {
        }

        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Cita> Citas { get; set; }
    }
}
