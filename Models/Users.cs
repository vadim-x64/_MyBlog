using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models;

public class Users
{
    [Key]
    public Guid Id { get; set; }

    public byte[]? Avatar { get; set; }

    [Required(ErrorMessage = "Ім'я обов'язкове")]
    [StringLength(50, ErrorMessage = "Ім'я не повинно перевищувати 50 символів")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Прізвище обов'язкове")]
    [StringLength(50, ErrorMessage = "Прізвище не повинно перевищувати 50 символів")]
    public string Surname { get; set; } = null!;

    [StringLength(50, ErrorMessage = "По батькові не повинно перевищувати 50 символів")]
    public string? LastName { get; set; }
    
    [StringLength(50, ErrorMessage = "Нікнейм не повинен перевищувати 50 символів")]
    public string? NickName { get; set; }

    [DataType(DataType.Date)]
    [Range(typeof(DateTime), "1900-01-01", "9999-12-31", ErrorMessage = "Дата народження повинна бути в межах від 01.01.1900 до поточної дати")]
    public DateTime? BirthDate { get; set; }

    [Required(ErrorMessage = "Номер телефону обов'язковий")]
    [Phone(ErrorMessage = "Неправильний формат телефону")]
    [StringLength(50, ErrorMessage = "Телефон не повинен перевищувати 50 символів")]
    public string PhoneNumber { get; set; } = null!;

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Неправильний формат email")]
    [StringLength(50, ErrorMessage = "Email не повинен перевищувати 50 символів")]
    public string Email { get; set; } = null!;

    [Required]
    [MaxLength(256)]
    public string PasswordHash { get; set; } = null!;
    
    [StringLength(150, ErrorMessage = "Інформація про себе не повинна перевищувати 150 символів")]
    public string? About { get; set; }
    
    public int Role { get; set; } = 1;
    
    public bool IsBlocked { get; set; } = false;
}