using MyBlog.Models;

namespace MyBlog.Repository.Interfaces;

public interface IPostService
{
    Task<List<Post>> GetAllPostsAsync();
    Task<Post?> GetPostByIdAsync(Guid id);
    Task<bool> CreatePostAsync(Post post, IFormFile? photo);
    Task<bool> UpdatePostAsync(Post updatedPost, IFormFile? photo);
    Task<bool> DeletePostAsync(Guid id);
    Task<List<Post>> GetUserPostsAsync(Guid userId);
    Task<List<Post>> GetPostsByUserIdAsync(Guid userId);
}