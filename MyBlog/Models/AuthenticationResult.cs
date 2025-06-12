namespace MyBlog.Models;

public class AuthenticationResult
{
    public bool IsSuccess { get; set; }
    
    public Users? User { get; set; }
    
    public bool IsBlocked { get; set; }
}