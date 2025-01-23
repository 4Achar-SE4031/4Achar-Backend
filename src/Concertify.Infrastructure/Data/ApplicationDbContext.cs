using System.Reflection.Emit;
using System;

using Concertify.Domain.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Concertify.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Concert> Concerts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }
    

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Concert>().HasKey(e => e.Id);
        builder.Entity<Concert>()
            .Property(c => c.StartDateTime)
            .HasConversion(
                src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                dest => dest.Kind == DateTimeKind.Utc ? dest : DateTime.SpecifyKind(dest, DateTimeKind.Utc)
            );


            // Many-to-many for comment likes
        builder.Entity<Comment>()
            .HasOne(comment => comment.User)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.UserId)
            // or .OnDelete(DeleteBehavior.Cascade) if you want
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Concert>()
            .HasMany(concert => concert.Comments)
            .WithOne(comment => comment.Event)
            .HasForeignKey(comment => comment.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}
