using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TeknikServis.Web.Controllers;

/// <summary>
/// Sidebar'daki "Cihaz Kataloğu" linki için stub. Gerçek işlevsellik CatalogController'da.
/// </summary>
[Authorize]
public class DeviceCatalogController : Controller
{
    [HttpGet]
    public IActionResult Index() => RedirectToAction("Index", "Catalog");
}
