using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Services;

namespace MyBlog.Pages;

public class DetailsModel : PageModel
{
    private readonly PostService _postService;
    private readonly CommentService _commentService;
    public List<Comment> Comments { get; set; } = new List<Comment>();

    [BindProperty]
    public string CommentContent { get; set; } = string.Empty;

    public Post? Post { get; set; }

    public DetailsModel(PostService postService, CommentService commentService)
    {
        _postService = postService;
        _commentService = commentService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Post = await _postService.GetPostByIdAsync(id);

        if (Post == null)
        {
            return NotFound();
        }

        Comments = await _commentService.GetCommentsForPostAsync(id);

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
}