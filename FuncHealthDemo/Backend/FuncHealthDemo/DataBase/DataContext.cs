using FuncHealthDemo.Entities;
using FuncHealthDemo.Enum;
using Microsoft.EntityFrameworkCore;

namespace FuncHealthDemo.DB;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    
    public DbSet<Entities.Task> Tasks => Set<Entities.Task>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure FbUserId as unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.FbUserId)
            .IsUnique();

        // Configure Task entity
        modelBuilder.Entity<Entities.Task>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Entities.Task>()
            .HasIndex(t => t.UserId);

        modelBuilder.Entity<Entities.Task>()
            .Property(t => t.Title)
            .HasMaxLength(200)
            .IsRequired();

        modelBuilder.Entity<Entities.Task>()
            .Property(t => t.Description)
            .HasMaxLength(2000);

        // Seed Users
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FbUserId = "mock-fb-uid-john-doe",
                Name = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "+1-555-0101",
                DateOfBirth = new DateTime(1985, 3, 15),
                Type = UserType.Client
            },
            new User
            {
                Id = 2,
                FbUserId = "mock-fb-uid-jane-smith",
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "+1-555-0102",
                DateOfBirth = new DateTime(1990, 7, 22),
                Type = UserType.Client
            }
        );

       
    }
}
