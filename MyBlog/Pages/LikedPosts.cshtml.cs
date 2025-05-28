using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Services;

namespace MyBlog.Pages;

[Authorize]
public class LikedPostsModel : PageModel
{
    private readonly LikeService _likeService;

    public List<Post> LikedPosts { get; set; } = new List<Post>();

    public LikedPostsModel(LikeService likeService)
    {
        _likeService = likeService;
    }

    public async Task OnGetAsync()
    {
        LikedPosts = await _likeService.GetLikedPostsByCurrentUserAsync();
    }

    public async Task<IActionResult> OnPostToggleLikeAsync(Guid postId)
    {
        await _likeService.ToggleLikeAsync(postId);
        return RedirectToPage();
    }
}