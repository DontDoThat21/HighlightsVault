using HighlightsVault.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace HighlightsVault
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Password>? Passwords { get; set; }
        public DbSet<Highlight>? Highlights { get; set; }
        public DbSet<HighlightsVaultGroup>? HighlightsVaultGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Password>().ToTable("Passwords"); // Explicitly map to table name
            modelBuilder.Entity<Password>().HasKey(p => p.Id); // Define primary key
            modelBuilder.Entity<Highlight>().ToTable("HighlightsVault"); // Explicitly map to table name
            modelBuilder.Entity<Highlight>().HasKey(p => p.ID); // Define primary key
            modelBuilder.Entity<Highlight>().Property(p => p.ID)
                .ValueGeneratedOnAdd(); // Define primary key
            modelBuilder.Entity<HighlightsVaultGroup>().HasKey(p => p.GroupId); // Define primary key
            modelBuilder.Entity<HighlightsVaultGroup>().Property(e => e.CreatedAt).IsRequired();

        }
    }
}
