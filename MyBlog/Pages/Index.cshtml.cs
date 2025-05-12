using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Services;

namespace MyBlog.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly PostService _postService;

    public List<Post> Posts { get; set; } = new List<Post>();

    public IndexModel(ILogger<IndexModel> logger, PostService postService)
    {
        _logger = logger;
        _postService = postService;
    }

    public async Task OnGetAsync()
    {
        Posts = await _postService.GetAllPostsAsync();
    }
}