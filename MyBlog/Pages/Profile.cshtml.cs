using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Services;

namespace MyBlog.Pages;

public class ProfileModel : PageModel
{
    private readonly UserService _userService;

    public ProfileModel(UserService userService)
    {
        _userService = userService;
    }

    public Users? CurrentUser { get; private set; }

    [BindProperty] public UpdateProfileModel UpdateModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        CurrentUser = await _userService.GetCurrentUserAsync();
        if (CurrentUser == null)
        {
            return RedirectToPage("/Login");
        }

        UpdateModel = new UpdateProfileModel
        {
            Name = CurrentUser.Name,
            Surname = CurrentUser.Surname,
            LastName = CurrentUser.LastName,
            NickName = CurrentUser.NickName,
            BirthDate = CurrentUser.BirthDate,
            PhoneNumber = CurrentUser.PhoneNumber,
            Email = CurrentUser.Email,
            About = CurrentUser.About
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        CurrentUser = await _userService.GetCurrentUserAsync();
        if (CurrentUser == null)
        {
            return RedirectToPage("/Login");
        }

        // Перевірка формату телефону (тільки цифри)
        if (!string.IsNullOrEmpty(UpdateModel.PhoneNumber) && !UpdateModel.PhoneNumber.All(char.IsDigit))
        {
            ModelState.AddModelError("UpdateModel.PhoneNumber", "Номер телефону має містити тільки цифри.");
            return Page();
        }

        if (UpdateModel.BirthDate > DateTime.Today)
        {
            ModelState.AddModelError("UpdateModel.BirthDate", "Дата народження не може бути пізніше поточної дати.");
            return Page();
        }

        if (UpdateModel.BirthDate < new DateTime(1900, 1, 1))
        {
            ModelState.AddModelError("UpdateModel.BirthDate", "Дата народження повинна бути не раніше 01.01.1900.");
            return Page();
        }

        bool isPasswordChanged = !string.IsNullOrWhiteSpace(UpdateModel.NewPassword);

        var (updated, errorType) = await _userService.UpdateUserAsync(CurrentUser.Id, UpdateModel);

        if (!updated)
        {
            if (errorType == "email")
            {
                ModelState.AddModelError("UpdateModel.Email", "Цей Email вже використовується.");
            }
            else if (errorType == "phone")
            {
                ModelState.AddModelError("UpdateModel.PhoneNumber", "Цей номер телефону вже використовується.");
            }
            else if (errorType == "nickname")
            {
                ModelState.AddModelError("UpdateModel.NickName", "Цей нікнейм вже зайнятий.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Не вдалося оновити дані профілю.");
            }

            return Page();
        }

        if (isPasswordChanged)
        {
            await _userService.SignOutAsync();
            TempData["PasswordChanged"] = "Пароль було успішно змінено. Будь ласка, увійдіть знову з новим паролем.";
            return RedirectToPage("/Login");
        }

        TempData["SuccessMessage"] = "Профіль успішно оновлено";

        CurrentUser = await _userService.GetCurrentUserAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadAvatarAsync(IFormFile avatar)
    {
        CurrentUser = await _userService.GetCurrentUserAsync();
        if (CurrentUser == null)
        {
            return RedirectToPage("/Login");
        }

        if (avatar != null && avatar.Length > 0)
        {
            var (success, errorMessage) = await _userService.SaveAvatarAsync(CurrentUser, avatar);
            if (!success)
            {
                TempData["ErrorMessage"] = errorMessage;
            }
            else
            {
                // Додаємо повідомлення про успішне завантаження аватара
                TempData["SuccessMessage"] = CurrentUser.Avatar == null
                    ? "Аватар успішно встановлено"
                    : "Аватар успішно оновлено";
            }
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAvatarAsync()
    {
        CurrentUser = await _userService.GetCurrentUserAsync();
        if (CurrentUser == null)
        {
            return RedirectToPage("/Login");
        }

        var success = await _userService.DeleteAvatarAsync(CurrentUser);
        if (!success)
        {
            TempData["ErrorMessage"] = "Не вдалося видалити аватар";
        }
        else
        {
            // Додаємо повідомлення про успішне видалення аватара
            TempData["SuccessMessage"] = "Аватар успішно видалено";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAccountAsync(string confirmPassword)
    {
        CurrentUser = await _userService.GetCurrentUserAsync();
        if (CurrentUser == null)
        {
            return RedirectToPage("/Login");
        }

        // Перевіряємо пароль перед видаленням акаунту
        var authResult = await _userService.AuthenticateAsync(CurrentUser.Email, confirmPassword);
        if (!authResult.IsSuccess || authResult.User?.Id != CurrentUser.Id)
        {
            TempData["ErrorMessage"] = "Невірний пароль. Акаунт не було видалено.";
            return RedirectToPage();
        }

        var success = await _userService.DeleteUserAccountAsync(CurrentUser.Id);
        if (!success)
        {
            TempData["ErrorMessage"] = "Не вдалося видалити акаунт";
            return RedirectToPage();
        }

        // Тут не потрібно додавати повідомлення про успіх, оскільки користувач буде перенаправлений на головну сторінку
        return RedirectToPage("/Index");
    }
}