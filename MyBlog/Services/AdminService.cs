using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Repository.Context;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public AdminService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }
    
    public async Task<List<Post>> GetAllPostsForModerationAsync()
    {
        return await _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }
    
    public async Task<Post?> GetPostByIdAsync(Guid postId)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }
    
    public async Task<bool> DeletePostByAdminAsync(Guid postId)
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        
        if (!isAdmin)
        {
            return false;
        }

        var post = await _context.Posts.FindAsync(postId);
        
        if (post == null)
        {
            return false;
        }
        
        var comments = await _context.Comments.Where(c => c.PostId == postId).ToListAsync();
        
        _context.Comments.RemoveRange(comments);
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<Comment>> GetAllCommentsForModerationAsync()
    {
        return await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .Include(c => c.ParentComment)
            .ThenInclude(p => p != null ? p.Author : null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<Comment?> GetCommentByIdAsync(Guid commentId)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .Include(c => c.ParentComment)
            .ThenInclude(p => p != null ? p.Author : null)
            .FirstOrDefaultAsync(c => c.Id == commentId);
    }
    
    public async Task<bool> DeleteCommentByAdminAsync(Guid commentId)
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        
        if (!isAdmin)
        {
            return false;
        }

        var comment = await _context.Comments.FindAsync(commentId);
        
        if (comment == null)
        {
            return false;
        }
        
        await DeleteChildCommentsAsync(commentId);
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }
    
    private async Task DeleteChildCommentsAsync(Guid parentCommentId)
    {
        var childComments = await _context.Comments
            .Where(c => c.ParentCommentId == parentCommentId)
            .ToListAsync();

        foreach (var child in childComments)
        {
            await DeleteChildCommentsAsync(child.Id);
            _context.Comments.Remove(child);
        }
    }
}