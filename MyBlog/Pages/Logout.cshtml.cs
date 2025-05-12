using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Services;

namespace MyBlog.Pages;

public class LogoutModel : PageModel
{
    private readonly UserService _userService;

    public LogoutModel(UserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await _userService.SignOutAsync();
        return RedirectToPage("/Index");
    }
}