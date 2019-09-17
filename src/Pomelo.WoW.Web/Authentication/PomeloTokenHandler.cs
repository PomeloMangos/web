using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pomelo.WoW.Web.Models;
using Dapper;

namespace Pomelo.WoW.Web.Authentication
{
    public class PomeloTokenHandler : AuthenticationHandler<PomeloTokenOptions>
    {
        public new const string Scheme = "Pomelo";

        public PomeloTokenHandler(
            IOptionsMonitor<PomeloTokenOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder, 
            IDataProtectionProvider dataProtection, 
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            return base.HandleForbiddenAsync(properties);
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            Context.Response.Redirect("/account/login");
            return Task.FromResult(0);
            //return base.HandleChallengeAsync(properties);
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var user = Context.Session.GetString("user");

            if (string.IsNullOrEmpty(user))
            {
                return AuthenticateResult.NoResult();
            }

            Account account = null;

            using (var conn = Account.GetAuthDb())
            {
                var query = await conn.QueryAsync<Account>(
                    "SELECT * FROM `pomelo_account`" +
                    "WHERE `Username` = @Username;", 
                    new { Username = user });

                if (query.Count() == 0)
                {
                    return AuthenticateResult.Fail(new Exception("User not found"));
                }

                account = query.First();
            }

            var claimIdentity = new ClaimsIdentity(Scheme, ClaimTypes.Name, ClaimTypes.Role);
            claimIdentity.AddClaim(new Claim(ClaimTypes.Name, user));
            claimIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()));
            claimIdentity.AddClaim(new Claim(ClaimTypes.Role, account.Role.ToString()));
            claimIdentity.AddClaim(new Claim(ClaimTypes.Email, account.Email));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(claimIdentity), Scheme);
            return AuthenticateResult.Success(ticket);
        }
    }
}
