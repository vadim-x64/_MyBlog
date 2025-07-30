using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Pages;

public class LoginModel : PageModel
{
    private readonly IUserService _userService;

    public LoginModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty]
    public LoginFormModel LoginForm { get; set; } = new();

    [TempData]
    public string ErrorMessage { get; set; }
    
    public bool IsUserBlocked { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }
        
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var authResult = await _userService.AuthenticateAsync(LoginForm.Identifier, LoginForm.Password);
        
        if (!authResult.IsSuccess)
        {
            if (authResult.IsBlocked)
            {
                IsUserBlocked = true;
                return RedirectToPage("/AccessDenied", new { blocked = true });
            }
            
            ErrorMessage = "Неправильний логін або пароль";
            return Page();
        }

        await _userService.SignInAsync(authResult.User);
        return RedirectToPage("/Index");
    }

    public class LoginFormModel
    {
        [Required(ErrorMessage = "Введіть email, телефон або нікнейм")]
        [Display(Name = "Email, телефон або нікнейм")]
        [StringLength(50, ErrorMessage = "Ідентифікатор не повинен перевищувати 50 символів")]
        public string Identifier { get; set; } = null!;

        [Required(ErrorMessage = "Введіть пароль")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        [MinLength(8, ErrorMessage = "Пароль повинен містити мінімум 8 символів")]
        public string Password { get; set; } = null!;
    }
}