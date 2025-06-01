using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Repository.Interfaces;
using MyBlog.Services;

namespace MyBlog.Pages;

[Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
public class UserManagerModel : PageModel
{
    private readonly IUserService _userService;

    public UserManagerModel(IUserService userService)
    {
        _userService = userService;
    }

    public List<Users> RegularUsers { get; private set; } = new();
    
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        if (!isAdmin)
        {
            return RedirectToPage("/AccessDenied");
        }

        RegularUsers = await _userService.GetAllRegularUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostToggleBlockAsync(Guid userId)
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        if (!isAdmin)
        {
            return RedirectToPage("/AccessDenied");
        }

        await _userService.ToggleUserBlockStatusAsync(userId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteUserAsync(Guid userId)
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        if (!isAdmin)
        {
            return RedirectToPage("/AccessDenied");
        }

        await _userService.DeleteUserByAdminAsync(userId);
        return RedirectToPage();
    }
    
    public async Task<IActionResult> OnPostDeleteAvatarAsync(Guid userId)
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        if (!isAdmin)
        {
            return RedirectToPage("/AccessDenied");
        }

        var success = await _userService.DeleteUserAvatarByAdminAsync(userId);
        if (success)
        {
            StatusMessage = "Аватар користувача успішно видалено";
        }
        else
        {
            StatusMessage = "Помилка при видаленні аватара користувача";
        }
        
        return RedirectToPage();
    }
}