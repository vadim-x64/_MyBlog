using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Pages;

public class LogoutModel : PageModel
{
    private readonly IUserService _userService;

    public LogoutModel(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await _userService.SignOutAsync();
        return RedirectToPage("/Index");
    }
}