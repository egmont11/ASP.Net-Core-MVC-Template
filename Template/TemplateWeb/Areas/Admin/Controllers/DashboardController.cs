using Microsoft.AspNetCore.Mvc;

namespace TemplateWeb.Areas.Admin.Controllers;

public class DashboardController : AdminBaseController
{
    public IActionResult Index()
    {
        return View();
    }
}
