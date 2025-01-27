using System.Reflection.Emit;
using System;

using Concertify.Domain.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


namespace Concertify.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Concert> Concerts { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Bookmark> Bookmarks { get; set; }

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
        builder.Entity<Rating>().HasKey(e => e.Id);

        builder.Entity<Rating>()
            .HasIndex(r => new {r.UserId, r.ConcertId })
            .IsUnique();
        builder.Entity<Bookmark>()
            .HasIndex(b => new { b.UserId, b.ConcertId })
            .IsUnique();

        builder.Entity<Concert>()
            .HasIndex(c => new
            {
                c.Title,
                c.StartDateTime,
                c.City,
            }).IsUnique();

        builder.Entity<Concert>()
            .HasMany(e => e.Ratings)
            .WithMany(e => e.RatedConcerts)
            .UsingEntity<Rating>(
                l => l.HasOne<ApplicationUser>().WithMany().HasForeignKey(e => e.UserId),
                r => r.HasOne<Concert>().WithMany().HasForeignKey(e => e.ConcertId));
        
        builder.Entity<ApplicationUser>()
            .HasMany(e => e.BookmarkedConcerts)
            .WithMany(c => c.Bookmarks)
            .UsingEntity<Bookmark>(
                l => l.HasOne<Concert>().WithMany().HasForeignKey(e => e.ConcertId),
                r => r.HasOne<ApplicationUser>().WithMany().HasForeignKey(e => e.UserId));

        builder.Entity<Concert>()
            .Property(c => c.StartDateTime)
            .HasConversion(
                src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                dest => dest.Kind == DateTimeKind.Utc ? dest : DateTime.SpecifyKind(dest, DateTimeKind.Utc)
            );


        builder.Entity<Comment>()
            .HasOne(comment => comment.User)
            .WithMany(user => user.Comments)
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Concert>()
            .HasMany(concert => concert.Comments)
            .WithOne(comment => comment.Event)
            .HasForeignKey(comment => comment.EventId)
            .OnDelete(DeleteBehavior.Cascade);
        
    }
}
