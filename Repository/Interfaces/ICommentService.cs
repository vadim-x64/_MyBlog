using MyBlog.Models;

namespace MyBlog.Repository.Interfaces;

public interface ICommentService
{
    Task<List<Comment>> GetCommentsForPostAsync(Guid postId);
    Task<bool> AddCommentAsync(string content, Guid postId, Guid? parentCommentId = null);
    Task<bool> DeleteCommentAsync(Guid commentId);
}