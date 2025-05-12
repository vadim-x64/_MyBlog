using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Repository.Context;
using MyBlog.Services;

namespace MyBlog.Pages;
[Microsoft.AspNetCore.Authorization.Authorize(Policy = "AdminOnly")]
public class CommentManagerModel : PageModel
{
    private readonly AdminService _adminService;
    private readonly UserService _userService;
    private readonly AppDbContext _context;

    public CommentManagerModel(AdminService adminService, UserService userService, AppDbContext context)
    {
        _adminService = adminService;
        _userService = userService;
        _context = context;
    }

    public List<Comment> AllComments { get; private set; } = new();
    
    [TempData]
    public string StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        if (!isAdmin)
        {
            return RedirectToPage("/AccessDenied");
        }

        // Завантажуємо коментарі з додатковою інформацією для модальних вікон
        AllComments = await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .ThenInclude(p => p.Author)
            .Include(c => c.ParentComment)
            .ThenInclude(p => p != null ? p.Author : null)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
            
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteCommentAsync(Guid commentId)
    {
        var isAdmin = await _userService.IsCurrentUserAdminAsync();
        if (!isAdmin)
        {
            return RedirectToPage("/AccessDenied");
        }

        var success = await _adminService.DeleteCommentByAdminAsync(commentId);
        
        if (success)
        {
            StatusMessage = "Коментар успішно видалено";
        }
        else
        {
            StatusMessage = "Помилка при видаленні коментаря";
        }
        
        return RedirectToPage();
    }
}