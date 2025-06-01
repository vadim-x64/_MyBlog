using MyBlog.Models;

namespace MyBlog.Repository.Interfaces;

public interface IAdminService
{
    Task<List<Post>> GetAllPostsForModerationAsync();
    Task<Post?> GetPostByIdAsync(Guid postId);
    Task<bool> DeletePostByAdminAsync(Guid postId);
    Task<List<Comment>> GetAllCommentsForModerationAsync();
    Task<Comment?> GetCommentByIdAsync(Guid commentId);
    Task<bool> DeleteCommentByAdminAsync(Guid commentId);
}