using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pomelo.WoW.Web.Models;
using Dapper;

namespace Pomelo.WoW.Web.Controllers
{
    public class ForumController : ControllerBase
    {
        public async Task<IActionResult> Index()
        {
            using (var conn = Forum.GetAuthDb())
            {
                var result = (await conn.QueryAsync<Forum>(
                    "SELECT * FROM `pomelo_forum_list` " +
                    "WHERE `ParentId` IS NULL " +
                    "ORDER BY `Priority` DESC;")).ToList();

                foreach (var x in result)
                {
                    x.SubForums = (await conn.QueryAsync<Forum>(
                        "SELECT * FROM `pomelo_forum_list` " +
                        "WHERE `ParentId` = @ParentId " +
                        "ORDER BY `Priority` DESC;", 
                        new { ParentId = x.Id })).ToList();
                }

                return View(result);
            }
        }
    }
}
