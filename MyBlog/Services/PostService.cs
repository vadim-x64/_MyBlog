using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Repository.Context;
using System.Net;
using System.Net.Http.Headers;

namespace MyBlog.Services;

public class PostService
{
    private readonly AppDbContext _context;
    private readonly UserService _userService;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string DEFAULT_PHOTO_URL = "https://img.freepik.com/premium-vector/collage-pictures-grid-photo-mood-board-vintage-portfolio-background-album-brandboard_171739-3448.jpg?semt=ais_hybrid&w=740";

    // Дозволені типи зображень
    private readonly string[] _allowedImageMimeTypes = { 
        "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp", "image/svg+xml", "image/tiff" 
    };

    public PostService(AppDbContext context, UserService userService, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _userService = userService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<Post>> GetAllPostsAsync()
    {
        return await _context.Posts
            .Include(p => p.Author)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAsync(Guid id)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> CreatePostAsync(Post post, IFormFile? photo)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return false;
        }

        post.Id = Guid.NewGuid();
        post.AuthorId = currentUser.Id;
        post.CreatedAt = DateTime.UtcNow.AddHours(+3);
        post.UpdatedAt = DateTime.UtcNow.AddHours(+3);
        
        // Обробка фото відповідно до вибраного типу
        if (post.UseLocalPhoto)
        {
            if (photo != null && photo.Length > 0 && IsValidImageFile(photo))
            {
                using var memoryStream = new MemoryStream();
                await photo.CopyToAsync(memoryStream);
                post.LocalPhoto = memoryStream.ToArray();
            }
            else
            {
                // Якщо локальне фото не завантажено або не є зображенням, використовуємо дефолтне
                post.UseLocalPhoto = false;
                post.RemotePhotoUrl = DEFAULT_PHOTO_URL;
            }
        }
        else
        {
            // Для віддаленого фото - перевіряємо валідність URL
            if (!string.IsNullOrEmpty(post.RemotePhotoUrl) && await IsValidImageUrlAsync(post.RemotePhotoUrl))
            {
                // URL є дійсним і посилається на зображення
            }
            else
            {
                post.RemotePhotoUrl = DEFAULT_PHOTO_URL;
            }
        }

        await _context.Posts.AddAsync(post);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdatePostAsync(Post updatedPost, IFormFile? photo)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return false;
        }

        var post = await _context.Posts.FindAsync(updatedPost.Id);
        if (post == null || post.AuthorId != currentUser.Id)
        {
            return false;
        }

        post.Title = updatedPost.Title;
        post.Content = updatedPost.Content;
        post.UpdatedAt = DateTime.UtcNow.AddHours(+3);
        post.UseLocalPhoto = updatedPost.UseLocalPhoto;
        
        // Обробка фото відповідно до вибраного типу
        if (post.UseLocalPhoto)
        {
            // Якщо вибрано локальне фото
            if (photo != null && photo.Length > 0 && IsValidImageFile(photo))
            {
                // Завантажено нове локальне фото
                using var memoryStream = new MemoryStream();
                await photo.CopyToAsync(memoryStream);
                post.LocalPhoto = memoryStream.ToArray();
            }
            else if (post.LocalPhoto == null || post.LocalPhoto.Length == 0)
            {
                // Якщо обрано локальне фото, але немає ні старого, ні нового
                post.UseLocalPhoto = false;
                post.RemotePhotoUrl = DEFAULT_PHOTO_URL;
            }
            // Інакше залишаємо існуюче локальне фото
        }
        else
        {
            // Вибрано віддалене фото
            if (!string.IsNullOrEmpty(updatedPost.RemotePhotoUrl) && await IsValidImageUrlAsync(updatedPost.RemotePhotoUrl))
            {
                post.RemotePhotoUrl = updatedPost.RemotePhotoUrl;
            }
            else
            {
                // Якщо URL порожній або невірний, встановлюємо дефолтне фото
                post.RemotePhotoUrl = DEFAULT_PHOTO_URL;
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeletePostAsync(Guid id)
    {
        var currentUser = await _userService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            return false;
        }

        var post = await _context.Posts.FindAsync(id);
        if (post == null || post.AuthorId != currentUser.Id)
        {
            return false;
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<List<Post>> GetUserPostsAsync(Guid userId)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }
    
    // Перевірка, чи файл є зображенням
    private bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;
            
        try 
        {
            // Перевіряємо MIME-тип файлу
            if (_allowedImageMimeTypes.Contains(file.ContentType))
                return true;
                
            // Додаткова перевірка шляхом читання заголовків файлу для визначення типу
            using var stream = file.OpenReadStream();
            var buffer = new byte[Math.Min(file.Length, 512)];
            stream.Read(buffer, 0, buffer.Length);
            var fileType = GetFileTypeFromHeader(buffer);
            return fileType.StartsWith("image/");
        }
        catch 
        {
            return false;
        }
    }
    
    // Метод для визначення типу файлу за заголовком
    private string GetFileTypeFromHeader(byte[] header)
    {
        // JPEG: FF D8 FF
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
            return "image/jpeg";
            
        // PNG: 89 50 4E 47 0D 0A 1A 0A
        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 
            && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
            return "image/png";
            
        // GIF: 47 49 46 38
        if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
            return "image/gif";
            
        // BMP: 42 4D
        if (header[0] == 0x42 && header[1] == 0x4D)
            return "image/bmp";
            
        // WEBP: 52 49 46 46 ?? ?? ?? ?? 57 45 42 50
        if (header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 
            && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
            return "image/webp";
            
        return "application/octet-stream"; // За замовчуванням - невідомий тип
    }
    
    // Перевірка URL на валідність і перевірка, що це URL зображення
    private async Task<bool> IsValidImageUrlAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
            return false;
            
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("BlogApp", "1.0"));
                
            // Надсилаємо HEAD-запит для перевірки доступності та типу ресурсу
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
                return false;
                
            // Перевіряємо, чи Content-Type це зображення
            if (response.Content.Headers.ContentType != null && 
                response.Content.Headers.ContentType.MediaType != null &&
                response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
            {
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}