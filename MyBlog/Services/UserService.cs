using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Repository.Context;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Users?> GetCurrentUserAsync()
    {
        if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return null;
        }

        return await _context.Users.FindAsync(Guid.Parse(userId));
    }

    public async Task<bool> IsCurrentUserAdminAsync()
    {
        var user = await GetCurrentUserAsync();
        return user?.Role == 2;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string identifier, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Email == identifier ||
            u.PhoneNumber == identifier ||
            u.NickName == identifier);

        if (user == null || user.PasswordHash != HashPassword(password))
        {
            return new AuthenticationResult { IsSuccess = false };
        }

        if (user.IsBlocked)
        {
            return new AuthenticationResult { IsSuccess = false, IsBlocked = true };
        }

        return new AuthenticationResult { IsSuccess = true, User = user };
    }

    public async Task<bool> RegisterUserAsync(Users user, string password)
    {
        var emailOrPhoneExists = await _context.Users.AnyAsync(u =>
            u.Email == user.Email ||
            u.PhoneNumber == user.PhoneNumber);

        var nicknameExists = !string.IsNullOrEmpty(user.NickName) &&
                             await _context.Users.AnyAsync(u => u.NickName == user.NickName);

        if (emailOrPhoneExists || nicknameExists)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(user.NickName))
        {
            user.NickName = GenerateUniqueNickname();
        }

        user.PasswordHash = HashPassword(password);
        user.Id = Guid.NewGuid();
        user.Role = 1;

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return true;
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

    public async Task<(bool Success, string? ErrorType)> UpdateUserAsync(Guid userId, UpdateProfileModel model)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return (false, null);
        }

        if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Surname) ||
            string.IsNullOrWhiteSpace(model.PhoneNumber) || string.IsNullOrWhiteSpace(model.Email))
        {
            return (false, null);
        }
        
        if (model.Email != user.Email &&
            await _context.Users.AnyAsync(u => u.Id != userId && u.Email == model.Email))
        {
            return (false, "email");
        }
        
        if (model.PhoneNumber != user.PhoneNumber &&
            await _context.Users.AnyAsync(u => u.Id != userId && u.PhoneNumber == model.PhoneNumber))
        {
            return (false, "phone");
        }
        
        if (!string.IsNullOrEmpty(model.NickName) && model.NickName != user.NickName &&
            await _context.Users.AnyAsync(u => u.Id != userId && u.NickName == model.NickName))
        {
            return (false, "nickname");
        }

        bool nickNameChanged = user.NickName != model.NickName;
        bool emailChanged = user.Email != model.Email;
        bool passwordChanged = !string.IsNullOrWhiteSpace(model.NewPassword);

        if (passwordChanged && model.NewPassword!.Length < 8)
        {
            return (false, null);
        }

        user.Name = model.Name;
        user.Surname = model.Surname;
        user.LastName = model.LastName;

        if (string.IsNullOrWhiteSpace(model.NickName))
        {
            user.NickName = GenerateUniqueNickname();
            nickNameChanged = true;
        }
        else
        {
            user.NickName = model.NickName;
        }

        user.BirthDate = model.BirthDate.HasValue
            ? DateTime.SpecifyKind(model.BirthDate.Value, DateTimeKind.Utc)
            : null;
        user.PhoneNumber = model.PhoneNumber;
        user.Email = model.Email;
        user.About = model.About;

        if (passwordChanged)
        {
            user.PasswordHash = HashPassword(model.NewPassword!);
        }

        await _context.SaveChangesAsync();

        if (!passwordChanged && (nickNameChanged || emailChanged))
        {
            await RefreshSignInAsync(user);
        }

        return (true, null);
    }

    public async Task RefreshSignInAsync(Users user)
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await SignInAsync(user);
    }

    private string HashPassword(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }

            return builder.ToString();
        }
    }

    public async Task SignInAsync(Users user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.NickName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FullName", $"{user.Surname} {user.Name} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };

        await _httpContextAccessor.HttpContext!.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }

    public async Task SignOutAsync()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }

    public async Task<(bool Success, string? ErrorMessage)> SaveAvatarAsync(Users user, IFormFile avatar)
    {
        if (avatar == null || avatar.Length == 0)
        {
            return (false, "Файл не вибрано");
        }
        
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".jfif" };
        string fileExtension = Path.GetExtension(avatar.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return (false, $"Дозволені формати зображень: {string.Join(", ", allowedExtensions)}");
        }

        using (var memoryStream = new MemoryStream())
        {
            await avatar.CopyToAsync(memoryStream);
            user.Avatar = memoryStream.ToArray();
            await _context.SaveChangesAsync();
            return (true, null);
        }
    }

    public async Task<bool> DeleteAvatarAsync(Users user)
    {
        if (user == null)
        {
            return false;
        }

        var dbUser = await _context.Users.FindAsync(user.Id);
        if (dbUser == null)
        {
            return false;
        }

        dbUser.Avatar = null;
        await _context.SaveChangesAsync();

        user.Avatar = null;
        return true;
    }

    public async Task<bool> DeleteUserAccountAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        await SignOutAsync();

        return true;
    }

    public async Task<List<Users>> GetAllRegularUsersAsync()
    {
        return await _context.Users
            .Where(u => u.Role == 1)
            .OrderBy(u => u.NickName)
            .ToListAsync();
    }

    public async Task<bool> ToggleUserBlockStatusAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Role == 2)
        {
            return false;
        }

        user.IsBlocked = !user.IsBlocked;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserByAdminAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || user.Role == 2)
        {
            return false;
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAvatarByAdminAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.Avatar = null;
        await _context.SaveChangesAsync();
        return true;
    }
}