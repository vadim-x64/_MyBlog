using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Repository.Interfaces;
using MyBlog.Services;

namespace MyBlog.Pages;

[Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
public class PostManagerModel : PageModel
{
    private readonly IAdminService _adminService;
    private readonly IUserService _userService;

    public PostManagerModel(IAdminService adminService, IUserService userService)
    {
        _adminService = adminService;
        _userService = userService;
    }

    public List<Post> AllPosts { get; private set; } = new();
    
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        if (!isAdmin)
        {
            return RedirectToPage("/AccessDenied");
        }

        AllPosts = await _adminService.GetAllPostsForModerationAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeletePostAsync(Guid postId)
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        if (!isAdmin)
        {
            return RedirectToPage("/AccessDenied");
        }

        var success = await _adminService.DeletePostByAdminAsync(postId);
        
        if (success)
        {
            StatusMessage = "Пост успішно видалено";
        }
        else
        {
            StatusMessage = "Помилка при видаленні поста";
        }
        
        return RedirectToPage();
    }
}