using Microsoft.AspNetCore.Mvc;

namespace Pomelo.WoW.Web.Controllers
{
    public class DownloadController : ControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
