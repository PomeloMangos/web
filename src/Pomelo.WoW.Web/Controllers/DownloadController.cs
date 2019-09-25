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

        public IActionResult Launcherwin7()
        {
            return File(System.IO.File.ReadAllBytes(Path.Combine("launcher", "launcher-win7.exe")), "application/x-msdownload", "柚子魔兽.exe");
        }
    }
}
