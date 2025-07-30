using MyBlog.Models;

namespace MyBlog.Repository.Interfaces;

public interface ILikeService
{
    Task<bool> ToggleLikeAsync(Guid postId);
    Task<bool> IsPostLikedByCurrentUserAsync(Guid postId);
    Task<int> GetLikesCountAsync(Guid postId);
    Task<List<Post>> GetLikedPostsByCurrentUserAsync();
}