using MyBlog.Models;

namespace MyBlog.Repository.Interfaces;

public interface IUserService
{
    Task<Users?> GetCurrentUserAsync();
    Task<bool> IsCurrentUserAdminAsync();
    Task<AuthenticationResult> AuthenticateAsync(string identifier, string password);
    Task<bool> RegisterUserAsync(Users user, string password);
    Task<(bool Success, string? ErrorType)> UpdateUserAsync(Guid userId, UpdateProfileModel model);
    Task RefreshSignInAsync(Users user);
    Task SignInAsync(Users user);
    Task SignOutAsync();
    Task<(bool Success, string? ErrorMessage)> SaveAvatarAsync(Users user, IFormFile avatar);
    Task<bool> DeleteAvatarAsync(Users user);
    Task<bool> DeleteUserAccountAsync(Guid userId);
    Task<List<Users>> GetAllRegularUsersAsync();
    Task<bool> ToggleUserBlockStatusAsync(Guid userId);
    Task<bool> DeleteUserByAdminAsync(Guid userId);
    Task<bool> DeleteUserAvatarByAdminAsync(Guid userId);
    Task<Users?> GetUserByIdAsync(Guid userId);
}