using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Repository.Interfaces;
using MyBlog.Services;

namespace MyBlog.Pages;

public class MyPostsModel : PageModel
{
    private readonly IPostService _postService;
    private readonly IUserService _userService;

    public List<Post> Posts { get; set; } = new List<Post>();

    public MyPostsModel(IPostService postService, IUserService userService)
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