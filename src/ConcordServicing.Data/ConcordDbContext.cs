using ConcordServicing.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ConcordServicing.Data;

public class ConcordDbContext : DbContext
{
    public ConcordDbContext(DbContextOptions<ConcordDbContext> options) : base(options)
    {
    }
    
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(map =>
        {
            map.ToTable("customers");
            map.HasKey(x => x.Id);
        });
    }
}
