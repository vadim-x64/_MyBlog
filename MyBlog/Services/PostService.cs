using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using MyBlog.Repository.Context;
using System.Net.Http.Headers;
using MyBlog.Repository.Interfaces;

namespace MyBlog.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string DEFAULT_PHOTO_URL = "https://img.freepik.com/premium-vector/collage-pictures-grid-photo-mood-board-vintage-portfolio-background-album-brandboard_171739-3448.jpg?semt=ais_hybrid&w=740";
    
    private readonly string[] _allowedImageMimeTypes = { 
        "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp", "image/svg+xml", "image/tiff" 
    };

    public PostService(AppDbContext context, IUserService userService, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _userService = userService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<Post>> GetAllPostsAsync()
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => !p.IsPrivate)
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
                post.UseLocalPhoto = false;
                post.RemotePhotoUrl = DEFAULT_PHOTO_URL;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(post.RemotePhotoUrl) && await IsValidImageUrlAsync(post.RemotePhotoUrl))
            {
                
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
        post.IsPrivate = updatedPost.IsPrivate; 
        
        if (post.UseLocalPhoto)
        {
            if (photo != null && photo.Length > 0 && IsValidImageFile(photo))
            {
                using var memoryStream = new MemoryStream();
                await photo.CopyToAsync(memoryStream);
                post.LocalPhoto = memoryStream.ToArray();
            }
            else if (post.LocalPhoto == null || post.LocalPhoto.Length == 0)
            {
                post.UseLocalPhoto = false;
                post.RemotePhotoUrl = DEFAULT_PHOTO_URL;
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(updatedPost.RemotePhotoUrl) && await IsValidImageUrlAsync(updatedPost.RemotePhotoUrl))
            {
                post.RemotePhotoUrl = updatedPost.RemotePhotoUrl;
            }
            else
            {
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
    
    private bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return false;
        }
        
        try 
        {
            if (_allowedImageMimeTypes.Contains(file.ContentType))
            {
                return true;
            }
            
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
    
    private string GetFileTypeFromHeader(byte[] header)
    {
        if (header == null || header.Length < 12)
        {
            return "application/octet-stream";
        }
        
        if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
        {
            if (header.Length > 10 && header[3] == 0xE0 && header[6] == 0x4A && header[7] == 0x46 && header[8] == 0x49 && header[9] == 0x46 && header[10] == 0x00)
            {
                return "image/jfif";
            }
            
            return "image/jpeg";
        }

        if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
        {
            return "image/png";
        }

        if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
        {
            return "image/gif";
        }

        if (header[0] == 0x42 && header[1] == 0x4D)
        {
            return "image/bmp";
        }
        
        return "application/octet-stream";
    }
    
    private async Task<bool> IsValidImageUrlAsync(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }
            
        try
        {
            var httpClient = _httpClientFactory.CreateClient(); 
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BlogApp", "1.0"));
            
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            
            if (response.Content.Headers.ContentType != null && response.Content.Headers.ContentType.MediaType != null && response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
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
    
    // Додати в PostService клас:

    public async Task<List<Post>> GetPostsByUserIdAsync(Guid userId)
    {
        return await _context.Posts
            .Include(p => p.Author)
            .Where(p => p.AuthorId == userId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }
}