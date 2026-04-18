using System.ComponentModel.DataAnnotations;

namespace TemplateWeb.Models;

public class ProfileViewModel
{
    [Required]
    [Display(Name = "Username")]
    [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "New Password (leave blank to keep current)")]
    [MinLength(5)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword")]
    public string? ConfirmPassword { get; set; }
}
