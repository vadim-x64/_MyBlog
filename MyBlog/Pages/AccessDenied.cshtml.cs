using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyBlog.Pages;

public class AccessDeniedModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public bool Blocked { get; set; }

    public void OnGet()
    {
        // Якщо користувач авторизований і намагається отримати доступ до захищеної сторінки,
        // сервер перенаправить його сюди автоматично.
        // Якщо користувач намагається отримати доступ безпосередньо до цієї сторінки,
        // але він не заблокований - перенаправляємо його на головну.
        if (!Blocked && User.Identity?.IsAuthenticated != true)
        {
            Response.Redirect("/");
        }
    }
}