using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Services;

namespace MyBlog.Pages;

public class MyPostsModel : PageModel
{
    private readonly PostService _postService;
    private readonly UserService _userService;

    public List<Post> Posts { get; set; } = new List<Post>();

    public MyPostsModel(PostService postService, UserService userService)
    {
        _postService = postService;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        
        if (currentUser == null)
        {
            return RedirectToPage("/Login");
        }
        
        Posts = await _postService.GetUserPostsAsync(currentUser.Id);
        return Page();
    }
}