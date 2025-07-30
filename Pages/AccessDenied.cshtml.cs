using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyBlog.Pages;

public class AccessDeniedModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool Blocked { get; set; }

    public IActionResult OnGet()
    {
        if (!Blocked)
        {
            return RedirectToPage("/Index");
        }
        
        return Page();
    }
}