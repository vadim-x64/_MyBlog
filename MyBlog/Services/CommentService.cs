using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Repository.Context;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public CommentService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<List<Comment>> GetCommentsForPostAsync(Guid postId)
    {
        return await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.ParentComment)
            .ThenInclude(p => p != null ? p.Author : null)
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> AddCommentAsync(string content, Guid postId, Guid? parentCommentId = null)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        
        if (currentUser == null)
        {
            return false;
        }

        var post = await _context.Posts.FindAsync(postId);
        
        if (post == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(content) || content.Length > 2000)
        {
            return false;
        }

        if (parentCommentId != null)
        {
            var parentComment = await _context.Comments.FindAsync(parentCommentId);
            
            if (parentComment == null)
            {
                return false;
            }
        }

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            Content = content.Trim(),
            AuthorId = currentUser.Id,
            PostId = postId,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow.AddHours(3)
        };

        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        
        if (currentUser == null)
        {
            return false;
        }

        var comment = await _context.Comments.FindAsync(commentId);
        
        if (comment == null || comment.AuthorId != currentUser.Id)
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