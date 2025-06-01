using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyBlog.Models;
using MyBlog.Services;
using System.ComponentModel.DataAnnotations;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Pages;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IPostService _postService;
    private readonly string[] _allowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".jfif" };
    private readonly string[] _allowedMimeTypes = { 
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp", "image/jfif" 
    };

    [BindProperty]
    public PostInputModel PostInput { get; set; } = new();

    [BindProperty]
    public IFormFile? Photo { get; set; }
    
    [TempData]
    public string? ErrorMessage { get; set; }

    public CreateModel(IPostService postService)
    {
        _postService = postService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Додаткова валідація для файлу, якщо вибрано локальне фото
        if (PostInput.UseLocalPhoto && Photo != null)
        {
            var extension = Path.GetExtension(Photo.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
            {
                ModelState.AddModelError("Photo", $"Файл має бути зображенням одного з форматів: {string.Join(", ", _allowedImageExtensions)}");
                return Page();
            }
            
            // Перевірка MIME-типу
            if (!_allowedMimeTypes.Contains(Photo.ContentType.ToLowerInvariant()))
            {
                ModelState.AddModelError("Photo", $"Завантажений файл не є зображенням дозволеного формату. Дозволено: {string.Join(", ", _allowedImageExtensions)}");
                return Page();
            }
        }
        
        // Перевірка URL для віддаленого фото
        if (!PostInput.UseLocalPhoto && !string.IsNullOrEmpty(PostInput.RemotePhotoUrl))
        {
            if (!Uri.TryCreate(PostInput.RemotePhotoUrl, UriKind.Absolute, out var uriResult) || 
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                ModelState.AddModelError("PostInput.RemotePhotoUrl", "Введений URL є недійсним. Буде використано стандартне зображення.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var post = new Post
        {
            Title = PostInput.Title,
            Content = PostInput.Content,
            RemotePhotoUrl = PostInput.RemotePhotoUrl,
            UseLocalPhoto = PostInput.UseLocalPhoto
        };

        var result = await _postService.CreatePostAsync(post, Photo);

        if (result)
        {
            return RedirectToPage("/Index");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Не вдалося створити пост. Спробуйте ще раз.");
            return Page();
        }
    }

    public class PostInputModel
    {
        [Required(ErrorMessage = "Заголовок обов'язковий")]
        [StringLength(50, ErrorMessage = "Заголовок не може перевищувати 50 символів")]
        public string Title { get; set; } = null!;
    
        [Url(ErrorMessage = "Введіть коректний URL")]
        public string? RemotePhotoUrl { get; set; }

        [Required(ErrorMessage = "Вміст обов'язковий")]
        [StringLength(5000, ErrorMessage = "Вміст не може перевищувати 5000 символів")]
        public string Content { get; set; } = null!;
        
        public bool UseLocalPhoto { get; set; } = true;
    }
}