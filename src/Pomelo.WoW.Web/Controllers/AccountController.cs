using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Pomelo.WoW.Web.Models;
using Pomelo.WoW.Web.Lib;
using Dapper;

namespace Pomelo.WoW.Web.Controllers
{
    public class AccountController : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var characters = await CharacterCollector.FindCharactersAsync(Account.Id);
            return View(characters);
        }

        public async Task<IActionResult> Index(uint realm, ulong character)
        {
            using (var conn = Account.GetAuthDb())
            {
                await SetDefaultCharacterAsync(Account.Id, realm, character, conn);
            }
            return Prompt(x =>
            {
                x.Title = "切换成功";
                x.Details = "您已经成功切换至新角色";
                x.HideBack = true;
                x.RedirectText = "刷新";
                x.RedirectUrl = Url.Action("Index");
            });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password, string confirm, string email, string contact)
        {
            using (var conn = Account.GetAuthDb())
            {
                var query = await conn.QueryAsync<int>(
                    "SELECT COUNT(1) FROM `pomelo_account` " +
                    "WHERE `username` = @username;", 
                    new { username });
                var count = query.First();
                if (count > 0)
                {
                    return Prompt(x =>
                    {
                        x.Title = "注册失败";
                        x.Details = $"用户名{username}已经被注册，请您更换后再尝试！";
                        x.StatusCode = 400;
                    });
                }
                query = await conn.QueryAsync<int>(
                   "SELECT COUNT(1) FROM `pomelo_account` " +
                   "WHERE `email` = @email;",
                   new { email });
                count = query.First();
                if (count > 0)
                {
                    return Prompt(x =>
                    {
                        x.Title = "注册失败";
                        x.Details = $"电子邮箱{email}已经被注册，请您更换后再尝试！";
                        x.StatusCode = 400;
                    });
                }

                var hash1 = SHA256.Generate(password);
                var hash2 = Lib.SRP6.Generate(username, password);

                var account = new Account
                {
                    Username = username,
                    Hash = hash1.hash,
                    Salt = hash1.salt,
                    Role = AccountLevel.Player,
                    Email = email,
                    Contact = contact
                };

                query = await conn.QueryAsync<int>(
                    "INSERT INTO `pomelo_account` (`Id`, `Username`, `Hash`, `Salt`, `Role`, `Email`, `Contact`) " +
                    "VALUES (@Id, @Username, @Hash, @Salt, @Role, @Email, @Contact);" +
                    "SELECT LAST_INSERT_ID();", account);

                await conn.ExecuteAsync(
                    "INSERT INTO `account` (`id`, `username`, `gmlevel`, `v`, `s`, `email`, `joindate`, `expansion`, `last_login`)" +
                    "VALUES (@entry, @username, @gmlevel, @v, @s, @email, @joindate, @expansion)",
                    new
                    {
                        id = query,
                        username = username.ToUpper(),
                        gmlevel = account.Role,
                        hash2.v,
                        hash2.s,
                        email = email.ToUpper(),
                        joindate = DateTime.UtcNow,
                        expansion = 1
                    });

                return Prompt(x => 
                {
                    x.Title = "注册成功";
                    x.Details = "您已经成功注册柚子通行证，您可以使用该账号登录游戏、论坛发帖、在网站上管理角色公会等。";
                    x.HideBack = true;
                    x.RedirectText = "登录";
                    x.RedirectUrl = Url.Action("Login");
                });
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            using (var conn = Account.GetAuthDb())
            {
                var query = await conn.QueryAsync<Account>(
                    "SELECT * FROM `pomelo_account`" +
                    "WHERE `username` = @username", new { username });
                var user = query.SingleOrDefault();

                if (user == null)
                {
                    return Prompt(x =>
                    {
                        x.Title = "登录失败";
                        x.Details = $"用户{username}不存在";
                        x.StatusCode = 404;
                    });
                }

                var validate = SHA256.Validate(username, password, user.Salt, user.Hash);
                if (!validate)
                {
                    return Prompt(x =>
                    {
                        x.Title = "登录失败";
                        x.Details = $"用户名或密码不正确";
                        x.StatusCode = 400;
                    });
                }

                // Set default character
                if (user.DefaultRealm.HasValue)
                {
                    var character = await CharacterCollector.GetCharacterAsync(user.DefaultRealm.Value, user.DefaultCharacter.Value);
                    if (character.AccountId != user.Id)
                    {
                        await SetDefaultCharacterAsync(user.Id, conn);
                    }
                }
                else
                {
                    await SetDefaultCharacterAsync(user.Id, conn);
                }

                HttpContext.Session.SetString("user", user.Username);
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("user");
            return RedirectToAction("Login");
        }

        private async Task SetDefaultCharacterAsync(ulong accountId, MySqlConnection conn)
        {
            var characters = await CharacterCollector.FindCharactersAsync(accountId);
            if (characters.Count() > 0)
            {
                await SetDefaultCharacterAsync(accountId, characters.First().RealmId, characters.First().Id, conn);
            }
            else
            {
                await conn.ExecuteAsync(
                "UPDATE `pomelo_account` " +
                "SET `DefaultRealm` = NULL, " +
                "`DefaultCharacter` = NULL " +
                "WHERE `Id` = @Id", new { Id = accountId });
            }
        }

        private async Task SetDefaultCharacterAsync(ulong accountId, uint realmId, ulong characterId, MySqlConnection conn)
        {
            await conn.ExecuteAsync(
                "UPDATE `pomelo_account` " +
                "SET `DefaultRealm` = @DefaultRealm, " +
                "`DefaultCharacter` = @DefaultCharacter " +
                "WHERE `Id` = @Id",
                new
                {
                    DefaultRealm = realmId,
                    DefaultCharacter = characterId,
                    Id = accountId
                });
        }
    }
}
