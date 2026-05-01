using TemplateWeb.Entities;

namespace TemplateWeb.Models;

public class UserWithRolesViewModel
{
    public UserEntity UserEntity { get; set; } = null!;
    public IList<string> Roles { get; set; } = [];
}
