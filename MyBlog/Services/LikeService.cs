using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Repository.Context;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Services;

public class LikeService : ILikeService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public LikeService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<bool> ToggleLikeAsync(Guid postId)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return false;
        }

        var existingLike = await _context.Likes
            .FirstOrDefaultAsync(l => l.UserId == currentUser.Id && l.PostId == postId);

        if (existingLike != null)
        {
            _context.Likes.Remove(existingLike);
        }
        else
        {
            var like = new Like
            {
                Id = Guid.NewGuid(),
                UserId = currentUser.Id,
                PostId = postId,
                CreatedAt = DateTime.UtcNow.AddHours(+3)
            };
            await _context.Likes.AddAsync(like);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsPostLikedByCurrentUserAsync(Guid postId)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return false;
        }

        return await _context.Likes
            .AnyAsync(l => l.UserId == currentUser.Id && l.PostId == postId);
    }

    public async Task<int> GetLikesCountAsync(Guid postId)
    {
        return await _context.Likes.CountAsync(l => l.PostId == postId);
    }

    public async Task<List<Post>> GetLikedPostsByCurrentUserAsync()
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return new List<Post>();
        }

        return await _context.Likes
            .Where(l => l.UserId == currentUser.Id)
            .Include(l => l.Post)
            .ThenInclude(p => p.Author)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => l.Post)
            .ToListAsync();
    }
}