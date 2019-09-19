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

        public async Task<IActionResult> Index(uint realmId, uint characterId)
        {
            using (var conn = Account.GetAuthDb())
            {
                var character = await CharacterCollector.GetCharacterAsync(realmId, characterId);
                await SetDefaultCharacterAsync(
                    Account.Id, 
                    realmId, 
                    characterId,
                    character.Name,
                    character.Race,
                    character.Class,
                    character.Level,
                    conn);
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
            if (password != confirm)
            {
                return Prompt(x =>
                {
                    x.Title = "注册失败";
                    x.Details = "两次密码输入不一致";
                    x.StatusCode = 400;
                });
            }

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
                    else
                    {
                        await SetDefaultCharacterAsync(user.Id, character.RealmId, 
                            character.Id, character.Name, character.Race, character.Class, 
                            character.Level, conn);
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

        [Authorize]
        [HttpGet]
        public IActionResult Stuck()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Stuck(int place)
        {
            if (Character.Faction == Faction.Alliance)
            {
                switch(place)
                {
                    case 1: // 暴风城
                        Character.Map = 0;
                        Character.X = -9065;
                        Character.Y = 434;
                        Character.Z = 94;
                        break;
                    case 2: // 铁炉堡
                        Character.Map = 0;
                        Character.X = -5032;
                        Character.Y = -819;
                        Character.Z = 495;
                        break;
                    case 3: // 达纳苏斯
                        Character.Map = 1;
                        Character.X = -9961;
                        Character.Y = 2055;
                        Character.Z = 1329;
                        break;
                    case 4: // 埃索达
                        Character.Map = 530;
                        Character.X = -4043.632813;
                        Character.Y = -11933.284180;
                        Character.Z = -0.057945;
                        break;
                }
            }
            else
            {
                switch (place)
                {
                    case 1: // 奥格瑞玛
                        Character.Map = 1;
                        Character.X = 1317;
                        Character.Y = -4383;
                        Character.Z = 27;
                        break;
                    case 2: // 雷霆崖
                        Character.Map = 1;
                        Character.X = -1292.98;
                        Character.Y = 143.898;
                        Character.Z = 129.83;
                        break;
                    case 3: // 幽暗城
                        Character.Map = 0;
                        Character.X = 1909;
                        Character.Y = 235;
                        Character.Z = 53;
                        break;
                    case 4: // 银月城
                        Character.Map = 530;
                        Character.X = 9336.9;
                        Character.Y = -7278.4;
                        Character.Z = 13.6;
                        break;
                }
            }

            using (var conn = Character.GetCharacterDb(Character.RealmId))
            {
                await conn.ExecuteAsync(
                    "UPDATE `characters` " +
                    "SET `position_x` = @X, " +
                    "`position_y` = @Y, " +
                    "`position_z` = @Z, " +
                    "`map` = @Map " +
                    "WHERE `guid` = @Id", 
                    new { Character.X, Character.Y, Character.Z, Character.Map, Character.Id });
            }

            return Prompt(x =>
            {
                x.Title = "角色卡死";
                x.Details = "系统已经将您的角色转移至安全区域，请您登录游戏查看。";
            });
        }

        [HttpGet]
        [Authorize]
        public IActionResult Gift()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Password()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Password(string password, string newpwd, string confirm)
        {
            if (newpwd != confirm)
            {
                return Prompt(x =>
                {
                    x.Title = "修改密码失败";
                    x.Details = "两次密码输入不一致";
                    x.StatusCode = 400;
                });
            }

            var validate = SHA256.Validate(User.Identity.Name, password, Account.Salt, Account.Hash);
            if (!validate)
            {
                return Prompt(x =>
                {
                    x.Title = "修改密码失败";
                    x.Details = "当前密码输入不正确";
                    x.StatusCode = 400;
                });
            }

            var hash1 = SHA256.Generate(newpwd);
            var hash2 = Lib.SRP6.Generate(User.Identity.Name, newpwd);

            using (var conn = Account.GetAuthDb())
            {
                await conn.ExecuteAsync(
                    "UPDATE `pomelo_account` SET `Hash` = @hash, `Salt` = @salt WHERE `Id` = @Id;" +
                    "UPDATE `account` SET `v` = @v, `s` = @s WHERE `id` = @Id;",
                    new { hash1.hash, hash1.salt, hash2.v, hash2.s, Account.Id });
            }

            return Prompt(x =>
            {
                x.Title = "修改密码成功";
                x.Details = "您已成功修改了密码，请使用新密码登录门户网站及游戏客户端。";
            });
        }

        private async Task SetDefaultCharacterAsync(ulong accountId, MySqlConnection conn)
        {
            var characters = await CharacterCollector.FindCharactersAsync(accountId);
            if (characters.Count() > 0)
            {
                await SetDefaultCharacterAsync(
                    accountId, 
                    characters.First().RealmId, 
                    characters.First().Id,
                    characters.First().Name,
                    characters.First().Race,
                    characters.First().Class,
                    characters.First().Level,
                    conn);
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

        private async Task SetDefaultCharacterAsync(ulong accountId, uint realmId, ulong characterId, string nickname, Race race, Class @class, int level, MySqlConnection conn)
        {
            await conn.ExecuteAsync(
                "UPDATE `pomelo_account` " +
                "SET `DefaultRealm` = @DefaultRealm, " +
                "`DefaultCharacter` = @DefaultCharacter, " +
                "`CharacterNickname` = @CharacterNickname, " +
                "`CharacterLevel` = @CharacterLevel, " +
                "`CharacterRace` = @CharacterRace, " +
                "`CharacterClass` = @CharacterClass " +
                "WHERE `Id` = @Id",
                new
                {
                    DefaultRealm = realmId,
                    DefaultCharacter = characterId,
                    Id = accountId,
                    CharacterNickname = nickname,
                    CharacterRace = race,
                    CharacterClass = @class,
                    CharacterLevel = level
                });
        }
    }
}
