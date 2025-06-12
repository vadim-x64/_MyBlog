using Microsoft.EntityFrameworkCore;
using MyBlog.Models;

namespace MyBlog.Repository.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<Users> Users { get; set; }
    
    public DbSet<Post> Posts { get; set; }
    
    public DbSet<Like> Likes { get; set; }
    
    public DbSet<Comment> Comments { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.PostId })
            .IsUnique();
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Post>()
            .HasOne(p => p.Author)
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.ParentComment)
            .WithMany(c => c.Replies)
            .HasForeignKey(c => c.ParentCommentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}