using CdrApix.Models;
using Microsoft.EntityFrameworkCore;

namespace CdrApix.Data
{
    public class CdrDbContext : DbContext
    {
        public CdrDbContext(DbContextOptions<CdrDbContext> options) : base(options) { }

        public DbSet<CdrRecord> CdrRecords { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CdrRecord>()
                .Property(p => p.Cost)
                .HasColumnType("decimal(18,3)");

            base.OnModelCreating(modelBuilder);
        }
    }
}
