using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Repository.Interfaces;
using MyBlog.Services;

namespace MyBlog.Pages;

public class DetailsModel : PageModel
{
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;
    private readonly ILikeService _likeService;
    
    public List<Comment> Comments { get; set; } = new List<Comment>();
    public Post? Post { get; set; }
    public bool IsLiked { get; set; }
    public int LikesCount { get; set; }

    [BindProperty]
    public string CommentContent { get; set; } = string.Empty;

    public DetailsModel(IPostService postService, ICommentService commentService, ILikeService likeService)
    {
        _postService = postService;
        _commentService = commentService;
        _likeService = likeService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Post = await _postService.GetPostByIdAsync(id);

        if (Post == null)
        {
            return NotFound();
        }

        Comments = await _commentService.GetCommentsForPostAsync(id);
        IsLiked = await _likeService.IsPostLikedByCurrentUserAsync(id);
        LikesCount = await _likeService.GetLikesCountAsync(id);

        return Page();
    }
    
    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (string.IsNullOrWhiteSpace(CommentContent))
        {
            return RedirectToPage(new { id });
        }

        await _commentService.AddCommentAsync(CommentContent, id);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddReplyAsync(Guid id, Guid parentCommentId, string replyContent)
    {
        if (!string.IsNullOrWhiteSpace(replyContent))
        {
            await _commentService.AddCommentAsync(replyContent, id, parentCommentId);
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(Guid id, Guid commentId)
    {
        await _commentService.DeleteCommentAsync(commentId);
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostToggleLikeAsync(Guid id)
    {
        await _likeService.ToggleLikeAsync(id);
        return RedirectToPage(new { id });
    }
}