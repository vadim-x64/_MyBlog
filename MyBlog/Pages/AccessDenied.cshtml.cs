using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyBlog.Pages;

public class AccessDeniedModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool Blocked { get; set; }

    public IActionResult OnGet()
    {
        // Якщо користувач не заблокований, перенаправляємо його на головну
        // Це забезпечує, що сторінка доступна тільки заблокованим користувачам
        if (!Blocked)
        {
            return RedirectToPage("/Index");
        }
        
        // Сюди потрапляємо тільки якщо користувач заблокований (Blocked = true)
        return Page();
    }
}