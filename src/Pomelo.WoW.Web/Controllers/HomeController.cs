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
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            using (var conn = await Announce.GetAuthDbAsync())
            {
                var query = await conn.QueryAsync<Announce>(
                    "SELECT * FROM `pomelo_announce` " +
                    "ORDER BY `Time` DESC LIMIT 10;");
                var ret = query.ToList();
                return View(ret);
            }
        }

        public async Task<IActionResult> Realmlist()
        {
            using (var conn = await Realm.GetAuthDbAsync())
            {
                var query = await conn.QueryAsync<Realm>(
                    "SELECT * FROM `realmlist`;");
                var ret = query.ToList();
                return View(ret);
            }
        }
    }
}
