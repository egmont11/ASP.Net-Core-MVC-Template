using System.ComponentModel.DataAnnotations;

namespace TemplateWeb.Models;

public class AuthLoginViewModel
{
    [Required(ErrorMessage = "Email or username is required")]
    [Display(Name = "Email or Username")]
    public string EmailOrUsername { get; set; } = "";
    
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    [Required] [MinLength(5)]
    public string Password { get; set; }

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; } = false;
    public string? ReturnUrl { get; set; }
}