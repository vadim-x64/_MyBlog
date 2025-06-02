using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IPostService _postService;
    private readonly ILikeService _likeService;

    public List<Post> Posts { get; set; } = new List<Post>();
    public Dictionary<Guid, bool> LikedPosts { get; set; } = new Dictionary<Guid, bool>();
    public Dictionary<Guid, int> LikesCount { get; set; } = new Dictionary<Guid, int>();

    public IndexModel(ILogger<IndexModel> logger, IPostService postService, ILikeService likeService)
    {
        _logger = logger;
        _postService = postService;
        _likeService = likeService;
    }

    public async Task OnGetAsync()
    {
        Posts = await _postService.GetAllPostsAsync();

        foreach (var post in Posts)
        {
            LikedPosts[post.Id] = await _likeService.IsPostLikedByCurrentUserAsync(post.Id);
            LikesCount[post.Id] = await _likeService.GetLikesCountAsync(post.Id);
        }
    }

    public async Task<IActionResult> OnPostToggleLikeAsync(Guid postId)
    {
        await _likeService.ToggleLikeAsync(postId);
        return RedirectToPage();
    }
}