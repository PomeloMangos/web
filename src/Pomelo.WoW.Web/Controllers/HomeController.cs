using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pomelo.WoW.Web.Models;
using Dapper;

namespace Pomelo.WoW.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        public async Task<IActionResult> Index()
        {
            using (var conn = Thread.GetAuthDb())
            {
                var query = await conn.QueryAsync<Thread>(
                    "SELECT * FROM `pomelo_forum_thread` " +
                    "WHERE `IsPinned` = TRUE " +
                    "ORDER BY `Time` DESC LIMIT 10;");
                var ret = query.ToList();
                return View(ret);
            }
        }

        public async Task<IActionResult> Realmlist()
        {
            using (var conn = Realm.GetAuthDb())
            {
                var query = await conn.QueryAsync<Realm>(
                    "SELECT * FROM `realmlist`;");
                var ret = query.ToList();
                return View(ret);
            }
        }

        public IActionResult Chat()
        {
            return View();
        }
    }
}
