using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace WebApplication1.Database
{
    public class RevoluDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Box> Boxes { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=revolu.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relation User -> Boxes
            modelBuilder.Entity<User>()
                .HasMany(u => u.Boxes)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId);

            // Relation Box -> Transactions
            modelBuilder.Entity<Box>()
                .HasMany(b => b.Transactions)
                .WithOne(t => t.Box)
                .HasForeignKey(t => t.BoxId);
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        
        public string Iban { get; set; }
        public string Bic { get; set; }
        
        public string? Token { get; set; }
        public List<Box> Boxes { get; set; } = new();
    }

    public class Box
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }

        public int UserId { get; set; }
        
      
        public User User { get; set; }

        public List<Transaction> Transactions { get; set; } = new();
    }

    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string CreatedAt { get; set; }
        public int BoxId { get; set; }
        public Box Box { get; set; }
    }
}