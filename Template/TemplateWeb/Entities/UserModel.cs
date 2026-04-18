using System.ComponentModel.DataAnnotations;

namespace TemplateWeb.Models.DbModels;

public class UserModel
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    [MinLength(3)]
    public string UserName { get; set; }
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    [Required]
    [MinLength(5)]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    
    public UserRole Role { get; set; } = UserRole.User;
}

public enum UserRole
{
    User,
    Admin
}