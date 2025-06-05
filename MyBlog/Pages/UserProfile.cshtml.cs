using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Pages;

public class UserProfileModel : PageModel
{
    private readonly IUserService _userService;
    private readonly IPostService _postService;

    public UserProfileModel(IUserService userService, IPostService postService)
    {
        _userService = userService;
        _postService = postService;
    }

    public Users? ProfileUser { get; set; }
    public List<Post> UserPosts { get; set; } = new List<Post>();
    public bool IsCurrentUser { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        
        // Якщо це наш власний профіль, перенаправляємо на сторінку Profile
        if (currentUser != null && currentUser.Id == id)
        {
            return RedirectToPage("/Profile");
        }

        ProfileUser = await _userService.GetUserByIdAsync(id);
        
        if (ProfileUser == null)
        {
            return NotFound("Користувача не знайдено");
        }

        UserPosts = await _postService.GetPostsByUserIdAsync(id);
        IsCurrentUser = currentUser?.Id == id;

        return Page();
    }
}