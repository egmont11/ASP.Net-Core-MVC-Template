using System.ComponentModel.DataAnnotations;

namespace TemplateWeb.Models;

public class AuthRegisterViewModel
{
    [Required] [Display(Name = "Username")] [StringLength(50, MinimumLength = 3)]
    public string UserName { get; set; }
    
    [Required] [EmailAddress] [Display(Name = "Email")]
    public string Email { get; set; }
    
    [Required] [DataType(DataType.Password)] [Display(Name = "Password")]
    public string Password { get; set; }
    
    [Display(Name = "Confirm password")]
    [Required] [DataType(DataType.Password)] [Compare("Password")]
    public string ConfirmPassword { get; set; }
    
    public string? ReturnUrl { get; set; }
}