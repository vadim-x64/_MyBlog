using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models;

public class Post
{
    [Key]
    public Guid Id { get; set; }

    public byte[]? LocalPhoto { get; set; }

    public string? RemotePhotoUrl { get; set; }
 
    public bool UseLocalPhoto { get; set; } = true;

    [Required]
    [StringLength(50, ErrorMessage = "Заголовок не може перевищувати 50 символів")]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(5000, ErrorMessage = "Вміст не може перевищувати 5000 символів")]
    public string Content { get; set; } = null!;

    [Required]
    public Guid AuthorId { get; set; }

    [ForeignKey("AuthorId")]
    public Users Author { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    
    public DateTime UpdatedAt { get; set; }
}