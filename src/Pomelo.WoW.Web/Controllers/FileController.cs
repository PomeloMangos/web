using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Pomelo.WoW.Web.Models;
using Dapper;

namespace Pomelo.WoW.Web.Controllers
{
    [Route("[controller]")]
    public class FileController : Controller
    {
        internal const int GuestLimit = 1024 * 512;

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Get(ulong id)
        {
            using (var conn = Blob.GetAuthDb())
            {
                var file = (await conn.QueryAsync<Blob>(
                    "SELECT * FROM `pomelo_blob` " +
                    "WHERE `Id` = @id;", new { id })).FirstOrDefault();

                if (file == null)
                {
                    Response.StatusCode = 404;
                    return File(new byte[] { }, "application/octet-stream");
                }

                return File(file.Bytes, file.ContentType, file.FileName);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Post(
            IFormFile file, string base64)
        {
            if (file != null)
            {
                if (!User.Identity.IsAuthenticated && file.Length > GuestLimit)
                {
                    Response.StatusCode = 400;
                    return Json("File is too large");
                }

                byte[] bytes = new byte[file.Length];
                using (var reader = new BinaryReader(file.OpenReadStream()))
                {
                    bytes = reader.ReadBytes(Convert.ToInt32(file.Length));
                }

                var blob = new Blob
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    ContentLength = file.Length,
                    Bytes = bytes
                };

                using (var conn = Blob.GetAuthDb())
                {
                    var id = (await conn.QueryAsync<uint>(
                        "INSERT INTO `pomelo_blob` " +
                        "(`FileName`, `ContentType`, `ContentLength`, `Bytes`) " +
                        "VALUES (@FileName, @ContentType, @ContentLength, @Bytes); " +
                        "SELECT LAST_INSERT_ID();", blob)).First();

                    return Json(new { id, blob.FileName });
                }

            }
            else if (base64 != null)
            {
                // TODO: Validate base 64 string format and return 4xx to client if it is invalid.

                if (!User.Identity.IsAuthenticated && base64.Length > GuestLimit)
                {
                    Response.StatusCode = 400;
                    return Json("File is too large");
                }

                var contentString = base64.Split(',')[1].Trim();
                var contentType = base64.Split(':')[1].Split(';')[0];
                var bytes = Convert.FromBase64String(contentString);

                var blob = new Blob
                {
                    FileName = "file",
                    ContentType = contentType,
                    ContentLength = bytes.Length,
                    Time = DateTime.UtcNow,
                    Bytes = bytes
                };

                using (var conn = Blob.GetAuthDb())
                {
                    var id = (await conn.QueryAsync<uint>(
                        "INSERT INTO `pomelo_blob` " +
                        "(`FileName`, `ContentType`, `ContentLength`, `Bytes`) " +
                        "VALUES (@FileName, @ContentType, @ContentLength, @Bytes); " +
                        "SELECT LAST_INSERT_ID();", blob)).First();

                    return Json(new { id, blob.FileName });
                }
            }
            else
            {
                Response.StatusCode = 400;
                return Json("Input format is incorrect");
            }
        }
    }

}
