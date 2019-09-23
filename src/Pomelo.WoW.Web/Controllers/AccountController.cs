using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Html;
using Newtonsoft.Json;
using MySql.Data.MySqlClient;
using Pomelo.WoW.Web.Models;
using Pomelo.WoW.Web.Lib;
using Dapper;

namespace Pomelo.WoW.Web.Controllers
{
    public class AccountController : ControllerBase
    {
        #region Player
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
                    "INSERT INTO `account` (`id`, `username`, `gmlevel`, `v`, `s`, `email`, `joindate`, `expansion`)" +
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

        [HttpGet]
        [Authorize]
        public IActionResult AvatarSettings()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AvatarSettings(IFormFile file)
        {
            if (file.Length > 1024 * 1024 * 1)
            {
                return Prompt(x =>
                {
                    x.Title = "头像上传失败";
                    x.Details = "头像必须小于1MB";
                    x.StatusCode = 400;
                });
            }

            using (var stream = file.OpenReadStream())
            using (var reader = new BinaryReader(file.OpenReadStream()))
            using (var conn = Account.GetAuthDb())
            {
                var bytes = reader.ReadBytes((int)file.Length);
                await conn.ExecuteAsync(
                    "UPDATE `pomelo_account` " +
                    "SET `Avatar` = @bytes " +
                    "WHERE `Id` = @Id;", 
                    new { bytes, Account.Id });

                return Prompt(x =>
                {
                    x.Title = "头像上传成功";
                    x.Details = "新的头像已生效";
                    x.HideBack = true;
                    x.RedirectText = "返回";
                    x.RedirectUrl = Url.Action("AvatarSettings");
                });
            }
        }

        [ResponseCache(Duration = 60 * 60 * 24 * 7)]
        [HttpGet("[controller]/{id}/avatar")]
        public async Task<IActionResult> Avatar(ulong id)
        {
            using (var conn = Account.GetAuthDb())
            {
                var bytes = (await conn.QueryAsync<byte[]>(
                    "SELECT `Avatar` FROM `pomelo_account` WHERE `Id` = @id", 
                    new { id })).SingleOrDefault();

                if (bytes == null || bytes.Length == 0)
                {
                    return File(System.IO.File.ReadAllBytes(System.IO.Path.Combine("wwwroot", "images", "avatar-neutral.jpg")), "application/octet-stream");
                }

                return File(bytes, "application/octet-stream");
            }
        }

        public IActionResult Move()
        {
            return Prompt(x =>
            {
                x.Title = "功能暂未开放";
                x.Details = "角色转移服务暂未开放，请您关注公告。";
            });
        }

        public IActionResult FindItem()
        {
            return Prompt(x =>
            {
                x.Title = "功能暂未开放";
                x.Details = "物品误删找回功能暂未开放，请您关注公告。";
            });
        }

        public IActionResult FindCharacter()
        {
            return Prompt(x =>
            {
                x.Title = "功能暂未开放";
                x.Details = "角色误删找回功能暂未开放，请您关注公告。";
            });
        }

        public IActionResult Transmog()
        {
            return Prompt(x =>
            {
                x.Title = "功能暂未开放";
                x.Details = "请在游戏内使用幻化功能，网页版暂未开放。";
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
        #endregion

        #region Administrator
        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult World()
        {
            return View(ModelBase.EnumerateWorldDbs());
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public IActionResult World(uint id)
        {
            HttpContext.Session.SetInt32("world", (int)id);
            return Prompt(x =>
            {
                x.Title = "选择成功";
                x.Details = "现在您可以对这个世界数据库进行操作了";
            });
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Parameter()
        {
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = Models.Parameter.GetWorldDb(db))
            {
                return View((await conn.QueryAsync<Parameter>("SELECT * FROM `pomelo_config`;")).ToList());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Parameter(string id, string value)
        {
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = Models.Parameter.GetWorldDb(db))
            {
                await conn.ExecuteAsync(
                    "UPDATE `pomelo_config` " +
                    "SET `value` = @value " +
                    "WHERE `entry` = @id;", new { id, value });
                return Prompt(x =>
                {
                    x.Title = "操作成功";
                    x.Details = "服务器参数已修改";
                });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Stone(uint id = 0)
        {
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = StoneMenu.GetWorldDb(db))
            {
                var menu = (await conn.QueryAsync<StoneMenu>(
                    "SELECT * FROM `pomelo_teleport_template` " +
                    "WHERE `menu_id` = @id;", new { id })).ToList();
                if (id != 0)
                {
                    ViewBag.Prev = await FindParentMenuAsync(id);
                }
                return View(menu);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditStone(uint menuId, uint actionId)
        {
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = StoneMenu.GetWorldDb(db))
            {
                var menu = (await conn.QueryAsync<StoneMenu>(
                    "SELECT * FROM `pomelo_teleport_template` " +
                    "WHERE `menu_id` = @menuId AND `action_id` = @actionId;", 
                    new { menuId, actionId })).SingleOrDefault();
                if (menu == null)
                {
                    return Prompt(x =>
                    {
                        x.Title = "没有找到菜单";
                        x.Details = "您指定的宝石菜单没有找到，请返回重试";
                        x.StatusCode = 404;
                    });
                }

                using (var conn2 = CustomCurrency.GetAuthDb())
                {
                    var currencies = await conn2.QueryAsync<CustomCurrency>(
                        "SELECT * FROM `pomelo_currency`;");
                    ViewBag.Currencies = currencies;
                }

                return View(menu);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> EditStone(uint id, string body)
        {
            var menu = JsonConvert.DeserializeObject<StoneMenu>(body);
            menu.Id = id;
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = StoneMenu.GetWorldDb(db))
            {
                var origin = (await conn.QueryAsync<StoneMenu>(
                    "SELECT * FROM `pomelo_teleport_template` " +
                    "WHERE `entry` = @id;",
                    new { id })).SingleOrDefault();

                var modifyId = "`menu_id` = @MenuId, `action_id` = @ActionId, ";
                if (origin.MenuId == menu.MenuId && origin.ActionId == menu.ActionId)
                {
                    modifyId = "";
                }

                await conn.ExecuteAsync(
                    "UPDATE `pomelo_teleport_template` " +
                    "SET `menu_item_text` = @MenuItemText, `icon` = @Icon, " +
                    modifyId +
                    "`function` = @Function, `teleport_map` = @TeleportMap, " +
                    "`teleport_x` = @TeleportX, `teleport_y` = @TeleportY, " +
                    "`teleport_z` = @TeleportZ, `cost_type` = @CostType, " +
                    "`cost_amount` = @CostAmount, `cost_custom_currency_id` = @CostCurrencyId, " +
                    "`level_required` = @LevelRequired, `permission_required` = @PermissionRequired, " +
                    "`trigger_menu` = @TriggerMenu, `faction_order` = @FactionOrder " +
                    "WHERE `entry` = @Id; ", menu);
                return Prompt(x =>
                {
                    x.Title = "编辑成功";
                    x.Details = "菜单已经成功保存";
                    x.RedirectText = "返回上级菜单";
                    x.RedirectUrl = Url.Action("Stone", new { id = menu.MenuId });
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> RemoveStone(uint id)
        {
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = StoneMenu.GetWorldDb(db))
            {
                var menuId = (await conn.QueryAsync<uint?>(
                    "SELECT `menu_id` FROM `pomelo_teleport_template` " +
                    "WHERE `entry` = @id;", new { id })).FirstOrDefault();

                if (!menuId.HasValue)
                {
                    return Prompt(x =>
                    {
                        x.Title = "删除失败";
                        x.Details = "您指定的菜单项没有找到";
                        x.StatusCode = 404;
                    });
                }

                await conn.ExecuteAsync(
                    "DELETE FROM `pomelo_teleport_template` " +
                    "WHERE `entry` = @id;", new { id });

                var cnt = (await conn.QueryAsync<int>(
                    "SELECT COUNT(1) FROM `pomelo_teleport_template` " +
                    "WHERE `menu_id` = @menuId", new { menuId = menuId.Value })).Single();

                uint? parentId = await FindParentMenuAsync(menuId.Value);

                return Prompt(x =>
                {
                    x.Title = "删除成功";
                    x.Details = "该菜单项已被成功删除";
                    x.RedirectText = cnt == 0 ? "返回上级菜单" : "返回菜单列表";
                    x.RedirectUrl = cnt == 0 ? Url.Action("Stone", new { id = parentId }) : Url.Action("Stone", new { id = menuId });
                });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public IActionResult CreateStone(int id)
        {
            ViewBag.MenuId = id;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> CreateStone(uint id, string body)
        {
            var menu = JsonConvert.DeserializeObject<StoneMenu>(body);
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = StoneMenu.GetWorldDb(db))
            {
                await conn.ExecuteAsync(
                    "INSERT INTO `pomelo_teleport_template` " +
                    "(`menu_id`, `action_id`, `icon`, `menu_item_text`, " +
                    "`teleport_x`, `teleport_y`, `teleport_z`, `teleport_map`, " +
                    "`function`, `cost_type`, `cost_amount`, " +
                    "`cost_custom_currency_id`, `level_required`, " +
                    "`permission_required`, `trigger_menu`, `faction_order`) " +
                    "VALUES (@MenuId, @ActionId, @Icon, @MenuItemText," +
                    "@TeleportX, @TeleportY, @TeleportZ, @TeleportMap, " +
                    "@Function, @CostType, @CostAmount, @CostCurrencyId," +
                    "@LevelRequired, @PermissionRequired, @TriggerMenu," +
                    "@FactionOrder);", menu);

                return Prompt(x =>
                {
                    x.Title = "创建成功";
                    x.Details = "新的菜单项已经创建";
                    x.RedirectText = "返回菜单列表";
                    x.RedirectUrl = Url.Action("Stone", new { id });
                });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Dungeon()
        {
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = StoneMenu.GetWorldDb(db))
            {
                var dungeons = (await conn.QueryAsync<DungeonSwitch>(
                    "SELECT * FROM `pomelo_dungeon_switch`;")).ToList();
                return View(dungeons);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Dungeon(uint id, bool status)
        {
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = StoneMenu.GetWorldDb(db))
            {
                await conn.ExecuteAsync(
                    "UPDATE `pomelo_dungeon_switch` " +
                    $"SET `disabled` = {(status ? "TRUE" : "FALSE")} " +
                    "WHERE `entry` = @id;", new { id });
                return Prompt(x =>
                {
                    x.Title = $"{(status ? "禁用" : "开启")}副本成功";
                    x.Details = "已更新副本开关状态";
                });
            }
        }

        private async Task<uint?> FindParentMenuAsync(uint menuId)
        {
            uint db = (uint)HttpContext.Session.GetInt32("world");
            using (var conn = StoneMenu.GetWorldDb(db))
            {
                var id = (await conn.QueryAsync<uint?>(
                    "SELECT `menu_id` " +
                    "FROM `pomelo_teleport_template` " +
                    "WHERE `trigger_menu` = @menuId;",
                    new { menuId })).FirstOrDefault();
                return id;
            }
        }
        #endregion
    }
}
