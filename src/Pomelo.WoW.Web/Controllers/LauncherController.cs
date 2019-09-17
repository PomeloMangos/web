using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.WoW.Web.Controllers
{
    [Route("[controller]")]
    public class LauncherController : ControllerBase
    {
        [HttpGet("register.url")]
        public IActionResult Register()
        {
            return Content(System.IO.File.ReadAllText("register.txt"));
        }

        [HttpGet("announce.html")]
        public IActionResult Announce()
        {
            return Content(System.IO.File.ReadAllText("announce.html"), "text/html");
        }

        [HttpGet("realm.json")]
        public IActionResult Realm()
        {
            return Content(System.IO.File.ReadAllText("realm.json"), "application/json");
        }

        [HttpGet("client.json")]
        public IActionResult ClientMeta()
        {
            return Content(System.IO.File.ReadAllText("client.json"), "application/json");
        }

        [HttpGet("download")]
        public IActionResult Download(string filename, long part = -1)
        {
            if (filename.Contains(".."))
            {
                return Content(string.Empty);
            }

            var fs = new FileStream(Path.Combine("client", filename), FileMode.Open, FileAccess.Read, FileShare.Read);
            if (part == -1)
            {
                return File(fs, "application/octet-stream", Path.GetFileName(filename), true);
            }
            else
            {
                fs.Seek(1024 * 1024 * 10 * part, SeekOrigin.Begin);
                var buffer = new byte[1024 * 1024 * 10];
                var read = fs.Read(buffer, 0, buffer.Length);
                return File(buffer.Take(read).ToArray(), "application/octet-stream", Path.GetFileName(filename) + ".part" + part);
            }
        }

        [HttpGet("hash")]
        public IActionResult Download(string filename)
        {
            if (filename.Contains(".."))
            {
                return Content(string.Empty);
            }

            using (var fs = new FileStream(Path.Combine("client", filename), FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var sha256 = SHA256.Create())
            {
                return Content(ByteArrayToHexString(sha256.ComputeHash(fs)));
            }
        }


        private static string ByteArrayToHexString(byte[] buf)
        {
            string returnStr = "";
            if (buf != null)
            {
                for (int i = 0; i < buf.Length; i++)
                {
                    returnStr += buf[i].ToString("X2");
                }
            }
            return returnStr;
        }
    }
}
