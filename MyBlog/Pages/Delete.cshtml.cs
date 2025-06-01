using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Repository.Interfaces;
using MyBlog.Services;

namespace MyBlog.Pages;

[Authorize]
public class DeleteModel : PageModel
{
    private readonly IPostService _postService;
    private readonly IUserService _userService;

    [BindProperty]
    public Post? Post { get; set; }

    public DeleteModel(IPostService postService, IUserService userService)
    {
        _postService = postService;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        Post = await _postService.GetPostByIdAsync(id);
        if (Post == null)
        {
            return NotFound();
        }

        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null || Post.AuthorId != currentUser.Id)
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Post == null)
        {
            return NotFound();
        }

        var result = await _postService.DeletePostAsync(Post.Id);

        if (result)
        {
            return RedirectToPage("/Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Не вдалося видалити пост. Перевірте, чи ви є автором.");
            return Page();
        }
    }
}