using Microsoft.EntityFrameworkCore;
using CARPARK.Model;


namespace CARPARK.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<CarPark_Main> CarPark_Mains { get; set; }
        public DbSet<CarPark_Detail> CarPark_Details { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CarPark_Main>().ToTable("CARPARK_MAIN");
            modelBuilder.Entity<CarPark_Detail>().ToTable("CARPARK_DETAIL");
        }
    }
}
