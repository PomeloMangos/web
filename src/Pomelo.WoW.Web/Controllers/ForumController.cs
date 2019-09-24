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
        public async Task<IActionResult> Thread(string id, int p = 1)
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
                    "`pomelo_account`.`Role`, `pomelo_forum_thread`.`ReplyCount` " +
                    "FROM `pomelo_forum_thread` " +
                    "INNER JOIN `pomelo_account` " +
                    "ON `pomelo_forum_thread`.`AccountId` = `pomelo_account`.`Id` " +
                    "WHERE (`ForumId` = @Id OR `IsPinned` = TRUE) " +
                    "AND `ParentId` IS NULL " +
                    "ORDER BY `IsPinned` DESC, `ReplyTime` DESC " +
                    "LIMIT 20 OFFSET {0}", (p - 1) * 20);

                var threads = (await conn.QueryAsync<Thread>(queryStr, new { forum.Id })).ToList();

                var count = (await conn.QueryAsync<int>(
                    "SELECT COUNT(1) FROM `pomelo_forum_thread` " +
                    "WHERE (`ForumId` = @Id OR `IsPinned` = TRUE) " +
                    "AND `ParentId` IS NULL;", new { forum.Id })).First();

                var pageCount = (count + 20 - 1) / 20;
                ViewBag.Count = count;
                ViewBag.PageCount = pageCount;
                ViewBag.Current = p;

                return View(threads);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(string title, string content, string forumId, uint? id, uint? parentId)
        {
            using (var conn = Models.Thread.GetAuthDb())
            {
                if (!id.HasValue)
                {
                    bool isLocked = false;
                    var rootId = parentId;
                    while (rootId.HasValue)
                    {
                        var tmp = (await conn.QueryAsync<uint?>(
                            "SELECT `ParentId` FROM `pomelo_forum_thread` " +
                            "WHERE `Id` = @id;", new { id = rootId.Value })).SingleOrDefault();
                        if (!tmp.HasValue)
                        {
                            isLocked = (await conn.QueryAsync<bool>(
                                "SELECT `IsLocked` FROM `pomelo_forum_thread` " +
                                "WHERE `Id` = @id;", new { id = rootId.Value })).SingleOrDefault();
                            break;
                        }
                        rootId = tmp;
                    }

                    if (isLocked && Account.Role < AccountLevel.Master)
                    {
                        return Prompt(x =>
                        {
                            x.Title = "回复失败";
                            x.Details = "该主题已被锁定";
                            x.StatusCode = 400;
                        });
                    }

                    if (parentId.HasValue)
                    {
                        await conn.ExecuteAsync(
                        "UPDATE `pomelo_forum_thread` " +
                        "SET `ReplyCount` = `ReplyCount` + 1 " +
                        "WHERE `Id` = @parentId ;", new { parentId });
                    }

                    var postId = (await conn.QueryAsync<uint>(
                        "INSERT INTO `pomelo_forum_thread` " +
                        "(`Title`, `Content`, `ForumId`, `AccountId`, `IsPinned`, `IsLocked`, `VisitCount`, `ParentId`) " +
                        "VALUES (@title, @content, @forumId, @accountId, FALSE, FALSE, 0, @parentId); " +
                        "SELECT LAST_INSERT_ID();",
                        new { title, content, forumId, accountId = Account.Id, parentId })).First();

                    if (parentId.HasValue)
                    {
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                    else
                    {
                        return RedirectToAction("Post", new { id = postId });
                    }
                }
                else
                {
                    var thread = (await conn.QueryAsync<Models.Thread>(
                        "SELECT `pomelo_forum_thread`.`Id`, `pomelo_forum_thread`.`ForumId`, " +
                        "`pomelo_forum_thread`.`Title`, `pomelo_forum_thread`.`VisitCount`, " +
                        "`pomelo_forum_thread`.`IsPinned`, `pomelo_forum_thread`.`IsLocked`, " +
                        "`pomelo_forum_thread`.`AccountId`, `pomelo_account`.`CharacterRace`, " +
                        "`pomelo_account`.`CharacterNickname`, `pomelo_account`.`Username` AS `AccountName`, " +
                        "`pomelo_account`.`CharacterClass`, `pomelo_account`.`CharacterLevel`, " +
                        "`pomelo_forum_thread`.`Time`, `pomelo_forum_thread`.`ReplyTime`, " +
                        "`pomelo_account`.`Role`, `pomelo_forum_thread`.`Content`, `pomelo_forum_thread`.`ReplyCount` " +
                        "FROM `pomelo_forum_thread` " +
                        "INNER JOIN `pomelo_account` " +
                        "ON `pomelo_forum_thread`.`AccountId` = `pomelo_account`.`Id` " +
                        "WHERE `pomelo_forum_thread`.`Id` = @id;", new { id = id.Value })).SingleOrDefault();

                    if (Account.Role < AccountLevel.Master && Account.Id != thread.AccountId)
                    {
                        return Prompt(x => 
                        {
                            x.Title = "权限不足";
                            x.Details = "您没有权限执行本操作";
                            x.StatusCode = 403;
                        });
                    }

                    await conn.ExecuteAsync(
                        "UPDATE `pomelo_forum_thread` " +
                        "SET `Content` = @content " +
                        "WHERE `Id` = @id;",
                        new { id, content });

                    return Redirect(Request.Headers["Referer"].ToString());
                }
            }
        }

        [HttpGet("thread/{id}")]
        public async Task<IActionResult> Post(uint id, int p = 1)
        {
            using (var conn = Models.Thread.GetAuthDb())
            {
                await conn.ExecuteAsync(
                "UPDATE `pomelo_forum_thread` " +
                "SET `VisitCount` = `VisitCount` + 1 " +
                "WHERE `Id` = @id", new { id });

                var thread = (await conn.QueryAsync<Models.Thread>(
                    "SELECT `pomelo_forum_thread`.`Id`, `pomelo_forum_thread`.`ForumId`, " +
                    "`pomelo_forum_thread`.`Title`, `pomelo_forum_thread`.`VisitCount`, " +
                    "`pomelo_forum_thread`.`IsPinned`, `pomelo_forum_thread`.`IsLocked`, " +
                    "`pomelo_forum_thread`.`AccountId`, `pomelo_account`.`CharacterRace`, " +
                    "`pomelo_account`.`CharacterNickname`, `pomelo_account`.`Username` AS `AccountName`, " +
                    "`pomelo_account`.`CharacterClass`, `pomelo_account`.`CharacterLevel`, " +
                    "`pomelo_forum_thread`.`Time`, `pomelo_forum_thread`.`ReplyTime`, " +
                    "`pomelo_account`.`Role`, `pomelo_forum_thread`.`Content` " +
                    "FROM `pomelo_forum_thread` " +
                    "INNER JOIN `pomelo_account` " +
                    "ON `pomelo_forum_thread`.`AccountId` = `pomelo_account`.`Id` " +
                    "WHERE `pomelo_forum_thread`.`Id` = @id;", new { id })).SingleOrDefault();

                if (thread == null)
                {
                    return Prompt(x =>
                    {
                        x.Title = "没有找到主题";
                        x.Details = "您指定的帖子没有找到，可能已被删除，请返回重试！";
                        x.StatusCode = 404;
                    });
                }

                ViewBag.Thread = thread;

                var ids = await conn.QueryAsync<uint>(
                    "SELECT `pomelo_forum_thread`.`Id` " +
                    "FROM `pomelo_forum_thread` " +
                    "WHERE `ParentId` = @ParentId " +
                    $"ORDER BY `Time` DESC LIMIT 10 OFFSET {(p - 1) * 10};",
                    new { ParentId = thread.Id });
                var idstr = string.Join(',', ids);
                if (string.IsNullOrEmpty(idstr))
                {
                    idstr = "0";
                }

                var queryStr = string.Format(
                    "SELECT `pomelo_forum_thread`.`Id`, `pomelo_forum_thread`.`ForumId`, " +
                    "`pomelo_forum_thread`.`Title`, `pomelo_forum_thread`.`VisitCount`, " +
                    "`pomelo_forum_thread`.`IsPinned`, `pomelo_forum_thread`.`IsLocked`, " +
                    "`pomelo_forum_thread`.`AccountId`, `pomelo_account`.`CharacterRace`, " +
                    "`pomelo_account`.`CharacterNickname`, `pomelo_account`.`Username` AS `AccountName`, " +
                    "`pomelo_account`.`CharacterClass`, `pomelo_account`.`CharacterLevel`, " +
                    "`pomelo_forum_thread`.`Time`, `pomelo_forum_thread`.`ReplyTime`, " +
                    "`pomelo_account`.`Role`, `pomelo_forum_thread`.`Content`, `pomelo_forum_thread`.`ParentId` " +
                    "FROM `pomelo_forum_thread` " +
                    "INNER JOIN `pomelo_account` " +
                    "ON `pomelo_forum_thread`.`AccountId` = `pomelo_account`.`Id` " +
                    "WHERE `pomelo_forum_thread`.`Id` IN ({0}) " +
                    "OR `pomelo_forum_thread`.`ParentId` IN ({0}) " +
                    "ORDER BY `Time` ASC;", idstr);

                var threadsPlain = (await conn.QueryAsync<Thread>(queryStr, new { id })).ToList();
                var threads = threadsPlain.Where(x => x.ParentId == thread.Id).ToList();
                foreach(var t in threads)
                {
                    t.SubThreads = threadsPlain.Where(x => x.ParentId == t.Id).ToList();
                }

                var count = (await conn.QueryAsync<int>(
                    "SELECT COUNT(1) FROM `pomelo_forum_thread` " +
                    "WHERE `ParentId` = @Id;", new { id })).First();

                var pageCount = (count + 10 - 1) / 10;
                ViewBag.Count = count;
                ViewBag.PageCount = pageCount;
                ViewBag.Current = p;


                ViewBag.ForumName = (await conn.QueryAsync<string>(
                    "SELECT `Name` FROM `pomelo_forum_list` " +
                    "WHERE `Id` = @ForumId;", new { thread.ForumId })).First();

                return View(threads);
            }
        }

        [Authorize]
        public async Task<IActionResult> Pin(uint id)
        {
            if (Account.Role < AccountLevel.Master)
            {
                return Prompt(x =>
                {
                    x.Title = "权限不足";
                    x.Details = "您没有权限执行本操作";
                    x.StatusCode = 403;
                });
            }

            using (var conn = Models.Thread.GetAuthDb())
            {
                await conn.ExecuteAsync(
                    "UPDATE `pomelo_forum_thread` " +
                    "SET `IsPinned` = NOT `IsPinned` " +
                    "WHERE `Id` = @id;", new { id });

                return Redirect(Request.Headers["Referer"].ToString());
            }
        }

        [Authorize]
        public async Task<IActionResult> Lock(uint id)
        {
            if (Account.Role < AccountLevel.Master)
            {
                return Prompt(x =>
                {
                    x.Title = "权限不足";
                    x.Details = "您没有权限执行本操作";
                    x.StatusCode = 403;
                });
            }

            using (var conn = Models.Thread.GetAuthDb())
            {
                await conn.ExecuteAsync(
                    "UPDATE `pomelo_forum_thread` " +
                    "SET `IsLocked` = NOT `IsLocked` " +
                    "WHERE `Id` = @id;", new { id });

                return Redirect(Request.Headers["Referer"].ToString());
            }
        }


        [Authorize]
        public async Task<IActionResult> Remove(uint id)
        {
            using (var conn = Models.Thread.GetAuthDb())
            {
                var thread = (await conn.QueryAsync<Models.Thread>(
                    "SELECT `pomelo_forum_thread`.`Id`, `pomelo_forum_thread`.`ForumId`, " +
                    "`pomelo_forum_thread`.`Title`, `pomelo_forum_thread`.`VisitCount`, " +
                    "`pomelo_forum_thread`.`IsPinned`, `pomelo_forum_thread`.`IsLocked`, " +
                    "`pomelo_forum_thread`.`AccountId`, `pomelo_account`.`CharacterRace`, " +
                    "`pomelo_account`.`CharacterNickname`, `pomelo_account`.`Username` AS `AccountName`, " +
                    "`pomelo_account`.`CharacterClass`, `pomelo_account`.`CharacterLevel`, " +
                    "`pomelo_forum_thread`.`Time`, `pomelo_forum_thread`.`ReplyTime`, " +
                    "`pomelo_account`.`Role`, `pomelo_forum_thread`.`Content`, `pomelo_forum_thread`.`ParentId` " +
                    "FROM `pomelo_forum_thread` " +
                    "INNER JOIN `pomelo_account` " +
                    "ON `pomelo_forum_thread`.`AccountId` = `pomelo_account`.`Id` " +
                    "WHERE `pomelo_forum_thread`.`Id` = @id;", new { id })).SingleOrDefault();

                if (Account.Role < AccountLevel.Master && Account.Id != thread.AccountId)
                {
                    return Prompt(x =>
                    {
                        x.Title = "权限不足";
                        x.Details = "您没有权限执行本操作";
                        x.StatusCode = 403;
                    });
                }

                await conn.ExecuteAsync("DELETE FROM `pomelo_forum_thread` WHERE `Id` = @id", new { id });

                if (thread.ParentId.HasValue)
                {
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                else
                {
                    return Prompt(x =>
                    {
                        x.Title = "删除成功";
                        x.Details = "该主题已被删除";
                        x.HideBack = true;
                        x.RedirectText = "返回主题列表";
                        x.RedirectUrl = Url.Action("Thread", new { id = thread.ForumId });
                    });
                }
            }
        }

        [HttpGet("markdown")]
        public async Task<IActionResult> Markdown(uint id)
        {
            using (var conn = Models.Thread.GetAuthDb())
            {
                return Content((await conn.QueryAsync<string>("SELECT `Content` FROM `pomelo_forum_thread` WHERE `Id` = @id", new { id })).SingleOrDefault() ?? "");
            }
        }
    }
}
