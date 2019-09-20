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
    public class ForumController : ControllerBase
    {
        [HttpGet]
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

        [HttpGet("forum/{id}")]
        public async Task<IActionResult> Thread(string id, int page = 1)
        {
            using (var conn = Forum.GetAuthDb())
            {
                var forum = (await conn.QueryAsync<Forum>(
                    "SELECT * FROM `pomelo_forum_list` WHERE `Id` = @id;", new { id })).SingleOrDefault();

                if (forum == null)
                {
                    return Prompt(x =>
                    {
                        x.Title = "没有找到板块";
                        x.Details = "您指定的论坛板块不存在，请更正后再试！";
                        x.StatusCode = 404;
                    });
                }

                ViewBag.Forum = forum;

                var queryStr = string.Format(
                    "SELECT `pomelo_forum_thread`.`Id`, `pomelo_forum_thread`.`ForumId`, " +
                    "`pomelo_forum_thread`.`Title`, `pomelo_forum_thread`.`VisitCount`, " +
                    "`pomelo_forum_thread`.`IsPinned`, `pomelo_forum_thread`.`IsLocked`, " +
                    "`pomelo_forum_thread`.`AccountId`, `pomelo_account`.`CharacterRace`, " +
                    "`pomelo_account`.`CharacterNickname`, `pomelo_account`.`Username` AS `AccountName`, " +
                    "`pomelo_account`.`CharacterClass`, `pomelo_account`.`CharacterLevel`, " +
                    "`pomelo_forum_thread`.`Time`, `pomelo_forum_thread`.`ReplyTime`, " +
                    "`pomelo_account`.`Role` " +
                    "FROM `pomelo_forum_thread` " +
                    "INNER JOIN `pomelo_account` " +
                    "ON `pomelo_forum_thread`.`AccountId` = `pomelo_account`.`Id` " +
                    "WHERE (`ForumId` = @Id OR `IsPinned` = TRUE) " +
                    "AND `ParentId` IS NULL " +
                    "ORDER BY `IsPinned` ASC, `ReplyTime` DESC " +
                    "LIMIT 20 OFFSET {0}", (page - 1) * 20);

                var threads = (await conn.QueryAsync<Thread>(queryStr, new { forum.Id })).ToList();

                var count = (await conn.QueryAsync<int>(
                    "SELECT COUNT(1) FROM `pomelo_forum_thread` " +
                    "WHERE (`ForumId` = @Id OR `IsPinned` = TRUE) " +
                    "AND `ParentId` IS NULL;", new { forum.Id })).First();

                var pageCount = (count + 20 - 1) / 20;
                ViewBag.Count = count;
                ViewBag.PageCount = pageCount;
                ViewBag.Current = page;

                return View(threads);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(string title, string content, string id)
        {
            using (var conn = Models.Thread.GetAuthDb())
            {
                var postId = (await conn.QueryAsync<uint>(
                    "INSERT INTO `pomelo_forum_thread` " +
                    "(`Title`, `Content`, `ForumId`, `AccountId`, `IsPinned`, `IsLocked`, `VisitCount`) " +
                    "VALUES (@title, @content, @id, @accountId, FALSE, FALSE, 0); " +
                    "SELECT LAST_INSERT_ID();", 
                    new { title, content, id, accountId = Account.Id })).First();

                return RedirectToAction("Post", new { id = postId });
            }
        }
    }
}
