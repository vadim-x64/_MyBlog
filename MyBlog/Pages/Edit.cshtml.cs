using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using System.ComponentModel.DataAnnotations;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Pages;

[Authorize]
public class EditModel : PageModel
{
    private readonly IPostService _postService;
    private readonly IUserService _userService;

    private readonly string[] _allowedImageExtensions =
        { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".tiff", ".tif" };

    [BindProperty] public PostInputModel PostInput { get; set; } = new();

    [BindProperty] public IFormFile? Photo { get; set; }

    public bool HasLocalPhoto { get; set; }
    public string? LocalPhotoBase64 { get; set; }

    [TempData] public string? ErrorMessage { get; set; }

    public EditModel(IPostService postService, IUserService userService)
    {
        _postService = postService;
        _userService = userService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var post = await _postService.GetPostByIdAsync(id);

        if (post == null)
        {
            return NotFound();
        }

        var currentUser = await _userService.GetCurrentUserAsync();

        if (currentUser == null || post.AuthorId != currentUser.Id)
        {
            return Forbid();
        }

        PostInput = new PostInputModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            RemotePhotoUrl = post.RemotePhotoUrl,
            UseLocalPhoto = post.UseLocalPhoto,
            IsPrivate = post.IsPrivate,
            CommentsDisabled = post.CommentsDisabled // Додайте цю строку
        };

        if (post.LocalPhoto != null && post.LocalPhoto.Length > 0)
        {
            HasLocalPhoto = true;
            LocalPhotoBase64 = Convert.ToBase64String(post.LocalPhoto);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (PostInput.UseLocalPhoto && Photo != null)
        {
            var extension = Path.GetExtension(Photo.FileName).ToLowerInvariant();

            if (!_allowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError("Photo", "Файл має бути зображенням (jpg, png, gif, bmp, webp)");
                return Page();
            }

            var allowedMimeTypes = new[]
            {
                "image/jpeg", "image/png", "image/gif", "image/bmp",
                "image/webp", "image/svg+xml", "image/tiff"
            };

            if (!allowedMimeTypes.Contains(Photo.ContentType))
            {
                ModelState.AddModelError("Photo", "Завантажений файл не є зображенням");

                var existingPost = await _postService.GetPostByIdAsync(PostInput.Id);

                if (existingPost != null && existingPost.LocalPhoto != null && existingPost.LocalPhoto.Length > 0)
                {
                    HasLocalPhoto = true;
                    LocalPhotoBase64 = Convert.ToBase64String(existingPost.LocalPhoto);
                }

                return Page();
            }
        }

        if (!PostInput.UseLocalPhoto && !string.IsNullOrEmpty(PostInput.RemotePhotoUrl))
        {
            if (!Uri.TryCreate(PostInput.RemotePhotoUrl, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("PostInput.RemotePhotoUrl",
                    "Введений URL є недійсним. Буде використано стандартне зображення.");
            }
        }

        if (!ModelState.IsValid)
        {
            var existingPost = await _postService.GetPostByIdAsync(PostInput.Id);

            if (existingPost != null && existingPost.LocalPhoto != null && existingPost.LocalPhoto.Length > 0)
            {
                HasLocalPhoto = true;
                LocalPhotoBase64 = Convert.ToBase64String(existingPost.LocalPhoto);
            }

            return Page();
        }

        // Отримуємо існуючий пост для збереження всіх полів
        var existingPostForUpdate = await _postService.GetPostByIdAsync(PostInput.Id);
        if (existingPostForUpdate == null)
        {
            return NotFound();
        }

        // Оновлюємо тільки ті поля, які можна редагувати
        existingPostForUpdate.Title = PostInput.Title;
        existingPostForUpdate.Content = PostInput.Content;
        existingPostForUpdate.RemotePhotoUrl = PostInput.RemotePhotoUrl;
        existingPostForUpdate.UseLocalPhoto = PostInput.UseLocalPhoto;
        existingPostForUpdate.IsPrivate = PostInput.IsPrivate;
        existingPostForUpdate.CommentsDisabled = PostInput.CommentsDisabled;

        var result = await _postService.UpdatePostAsync(existingPostForUpdate, Photo);

        if (result)
        {
            return RedirectToPage("/Details", new { id = existingPostForUpdate.Id });
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Не вдалося оновити пост. Перевірте, чи ви є автором.");

            if (existingPostForUpdate.LocalPhoto != null && existingPostForUpdate.LocalPhoto.Length > 0)
            {
                HasLocalPhoto = true;
                LocalPhotoBase64 = Convert.ToBase64String(existingPostForUpdate.LocalPhoto);
            }

            return Page();
        }
    }

    public class PostInputModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Заголовок обов'язковий")]
        [StringLength(50, ErrorMessage = "Заголовок не може перевищувати 50 символів")]
        public string Title { get; set; } = null!;

        [Url(ErrorMessage = "Введіть коректний URL")]
        public string? RemotePhotoUrl { get; set; }

        [Required(ErrorMessage = "Вміст обов'язковий")]
        [StringLength(5000, ErrorMessage = "Вміст не може перевищувати 5000 символів")]
        public string Content { get; set; } = null!;

        public bool UseLocalPhoto { get; set; } = true;
        public bool IsPrivate { get; set; } = false; // Додати цю строку

        public bool CommentsDisabled { get; set; } = false;
    }
}