using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Pomelo.WoW.Web.Models;
using Dapper;

namespace Pomelo.WoW.Web.Controllers
{
    public class AboutController : ControllerBase
    {
        protected override void Prepare()
        {
            base.Prepare();

            using (var conn = About.GetAuthDb())
            {
                var result = conn.Query<About>(
                    "SELECT `Id`, `Group`, `Title` " +
                    "FROM `pomelo_about` " +
                    "ORDER BY `Group`, `Priority`;").ToList();

                ViewBag.AboutItems = result;
            }
        }

        public async Task<IActionResult> Index(uint? id = null)
        {
            using (var conn = About.GetAuthDb())
            {
                List<About> result = ViewBag.AboutItems;
                if (!id.HasValue && result.Count == 0 || id.HasValue && !result.Any(x => x.Id == id.Value))
                {
                    return Prompt(x =>
                    {
                        x.Title = "没有找到该页面";
                        x.Details = "您访问的页面没有找到，请您返回后重试";
                        x.StatusCode = 404;
                    });
                }

                if (!id.HasValue)
                {
                    id = result.First().Id;
                }

                return View((await conn.QueryAsync<About>(
                    "SELECT * FROM `pomelo_about` " +
                    "WHERE `Id` = @id;", new { id = id.Value })).First());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(uint id)
        {
            using (var conn = About.GetAuthDb())
            {
                return View((await conn.QueryAsync<About>(
                    "SELECT * FROM `pomelo_about` " +
                    "WHERE `Id` = @id;", new { id = id })).First());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(uint id, string title, string group, string content, int priority)
        {
            using (var conn = About.GetAuthDb())
            {
                await conn.ExecuteAsync(
                    "UPDATE `pomelo_about` " +
                    "SET `Title` = @title, `Group` = @group, " +
                    "`Content` = @content, `Priority` = @priority " +
                    "WHERE `Id` = @id;", new { title, group, content, priority, id });
                return RedirectToAction("Index", new { id = id });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult Create() => View();

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create(string title, string group, string content, int priority)
        {
            using (var conn = About.GetAuthDb())
            {
                var id = await conn.QueryAsync<int>(
                    "INSERT INTO `pomelo_about` " +
                    "(`title`, `group`, `content`, `priority`) VALUES" +
                    "(@title, @group, @content, @priority); " +
                    "SELECT LAST_INSERT_ID();", 
                    new { title, group, content, priority });
                return RedirectToAction("Index", new { id = id.First() });
            }
        }
    }
}
