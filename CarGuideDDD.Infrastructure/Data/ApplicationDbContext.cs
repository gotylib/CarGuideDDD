using CarGuideDDD.Core.EntityObjects;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarGuideDDD.Infrastructure.Data
{
    public sealed class ApplicationDbContext : IdentityDbContext<EntityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<EntityCar> Cars { get; set; } = null!;

        public DbSet<EntityEndpointStatistics> EndpointStatistics { get; set; } = null!;
    }
}
