using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.WoW.Web.Controllers
{
    public class DownloadController : ControllerBase
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Launcher()
        {
            return File(System.IO.File.ReadAllBytes(Path.Combine("launcher", "launcher.exe")), "application/x-msdownload", "柚子魔兽.exe");
        }
    }
}
