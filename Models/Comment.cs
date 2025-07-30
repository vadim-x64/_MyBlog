using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models;

public class Comment
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(2000, ErrorMessage = "Коментар не може перевищувати 2000 символів")]
    public string Content { get; set; } = null!;

    [Required]
    public Guid AuthorId { get; set; }

    [ForeignKey("AuthorId")]
    public Users Author { get; set; } = null!;

    [Required]
    public Guid PostId { get; set; }

    [ForeignKey("PostId")]
    public Post Post { get; set; } = null!;

    public Guid? ParentCommentId { get; set; }

    [ForeignKey("ParentCommentId")]
    public Comment? ParentComment { get; set; }

    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public DateTime CreatedAt { get; set; }
}