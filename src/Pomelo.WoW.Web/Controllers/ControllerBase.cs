using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Pomelo.WoW.Web.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Pomelo.WoW.Web.Controllers
{
    public class ControllerBase : Controller
    {
        private Account _account;
        protected Account Account
        {
            get
            {
                if (_account == null && User.Identity.IsAuthenticated)
                {
                    using (var conn = Account.GetAuthDb())
                    {
                        _account = conn.Query<Account>(
                            "SELECT * FROM `pomelo_account`" +
                            "WHERE `Username` = @Username",
                            new { Username = User.Identity.Name }).Single();
                    }
                }
                return _account;
            }
        }

        private Character _character;
        protected Character Character
        {
            get
            {
                if (_character == null && User.Identity.IsAuthenticated && Account.DefaultRealm.HasValue)
                {
                    using (var conn = Character.GetCharacterDb(Account.DefaultRealm.Value))
                    {
                        _character = conn.Query<Character>(
                            "SELECT * FROM `characters`" +
                            "WHERE `guid` = @CharacterId",
                            new { CharacterId = Account.DefaultCharacter.Value }).SingleOrDefault();
                        _character.RealmId = Account.DefaultRealm.Value;
                    }
                }
                return _character;
            }
        }

        protected virtual void Prepare()
        {
            ViewBag.Account = Account;
            ViewBag.Character = Character;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Prepare();
            base.OnActionExecuting(context);
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Prepare();
            return base.OnActionExecutionAsync(context, next);
        }

        protected IActionResult Prompt(Action<Prompt> setupPrompt)
        {
            var prompt = new Prompt();
            setupPrompt(prompt);
            Response.StatusCode = prompt.StatusCode;
            return View("_Prompt", prompt);
        }
    }
}
