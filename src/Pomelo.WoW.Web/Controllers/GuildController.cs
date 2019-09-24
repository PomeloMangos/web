using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Pomelo.WoW.Web.Controllers
{
    public class GuildController : ControllerBase
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return Prompt(x =>
            {
                x.Title = "页面施工中";
                x.Details = "工程师正在努力研发这个页面";
            });
        }
    }
}
