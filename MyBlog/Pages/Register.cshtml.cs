using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Repository.Interfaces;
using MyBlog.Services;

namespace MyBlog.Pages;

public class RegisterModel : PageModel
{
    private readonly IUserService _userService;

    public RegisterModel(IUserService userService)
    {
        _userService = userService;
    }

    [BindProperty] public RegisterFormModel RegisterForm { get; set; } = new();

    [TempData] public string ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        RegisterForm.BirthDate = DateTime.Today.AddYears(-18);

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

        if (RegisterForm.Avatar != null)
        {
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".jfif" };
            string fileExtension = Path.GetExtension(RegisterForm.Avatar.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ErrorMessage = $"Дозволені формати зображень: {string.Join(", ", allowedExtensions)}";
                return Page();
            }
        }
        
        string nickname = RegisterForm.NickName;
        if (string.IsNullOrWhiteSpace(nickname))
        {
            nickname = GenerateUniqueNickname();
        }

        var user = new Users
        {
            Name = RegisterForm.Name,
            Surname = RegisterForm.Surname,
            LastName = RegisterForm.LastName,
            NickName = RegisterForm.NickName,
            BirthDate = RegisterForm.BirthDate.HasValue ? DateTime.SpecifyKind(RegisterForm.BirthDate.Value, DateTimeKind.Utc) : null,
            PhoneNumber = RegisterForm.PhoneNumber,
            Email = RegisterForm.Email,
            About = RegisterForm.About
        };

        var registrationSuccess = await _userService.RegisterUserAsync(user, RegisterForm.Password);

        if (!registrationSuccess)
        {
            ErrorMessage = "Користувач з такою поштою, телефоном або нікнеймом вже існує";
            return Page();
        }

        if (RegisterForm.Avatar != null)
        {
            var (success, errorMessage) = await _userService.SaveAvatarAsync(user, RegisterForm.Avatar);
            if (!success)
            {
                await _userService.DeleteUserAccountAsync(user.Id);
                ErrorMessage = errorMessage;
                return Page();
            }
        }

        await _userService.SignInAsync(user);
        return RedirectToPage("/Index");
    }

    private string GenerateUniqueNickname()
    {
        Random random = new Random();
        string randomDigits = string.Empty;

        for (int i = 0; i < 5; i++)
        {
            randomDigits += random.Next(0, 10).ToString();
        }

        return $"user_01{randomDigits}";
    }

    public class RegisterFormModel
    {
        [Display(Name = "Аватар")] public IFormFile? Avatar { get; set; }

        [Required(ErrorMessage = "Ім'я обов'язкове")]
        [Display(Name = "Ім'я")]
        [StringLength(50, ErrorMessage = "Ім'я не повинно перевищувати 50 символів")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Прізвище обов'язкове")]
        [Display(Name = "Прізвище")]
        [StringLength(50, ErrorMessage = "Прізвище не повинно перевищувати 50 символів")]
        public string Surname { get; set; } = null!;

        [Display(Name = "По батькові")]
        [StringLength(50, ErrorMessage = "По батькові не повинно перевищувати 50 символів")]
        public string? LastName { get; set; }

        [Display(Name = "Нікнейм")]
        [StringLength(50, ErrorMessage = "Нікнейм не повинен перевищувати 50 символів")]
        public string? NickName { get; set; }

        [Display(Name = "Дата народження")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; } = DateTime.Today.AddYears(-18);

        [Required(ErrorMessage = "Номер телефону обов'язковий")]
        [Phone(ErrorMessage = "Неправильний формат телефону")]
        [Display(Name = "Телефон")]
        [StringLength(50, ErrorMessage = "Телефон не повинен перевищувати 50 символів")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Неправильний формат email")]
        [Display(Name = "Email")]
        [StringLength(50, ErrorMessage = "Email не повинен перевищувати 50 символів")]
        public string Email { get; set; } = null!;

        [Display(Name = "Про себе")]
        [StringLength(150, ErrorMessage = "Інформація про себе не повинна перевищувати 150 символів.")]
        public string? About { get; set; }

        [Required(ErrorMessage = "Пароль обов'язковий")]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        [MinLength(8, ErrorMessage = "Пароль повинен містити мінімум 8 символів")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Підтвердження пароля обов'язкове")]
        [Compare("Password", ErrorMessage = "Паролі не співпадають")]
        [DataType(DataType.Password)]
        [Display(Name = "Підтвердження пароля")]
        [MinLength(8, ErrorMessage = "Пароль повинен містити мінімум 8 символів")]
        public string ConfirmPassword { get; set; } = null!;
    }
}