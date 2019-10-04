using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Pomelo.WoW.Web.Controllers
{
    public class AboutController : ControllerBase
    {
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
