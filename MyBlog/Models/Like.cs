using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models;

public class Like
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [ForeignKey("UserId")]
    public Users User { get; set; } = null!;

    [Required]
    public Guid PostId { get; set; }

    [ForeignKey("PostId")]
    public Post Post { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}