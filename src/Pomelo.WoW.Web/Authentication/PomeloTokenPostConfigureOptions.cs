using Microsoft.Extensions.Options;

namespace Pomelo.WoW.Web.Authentication
{
    /// <summary>
    /// Used to setup defaults for all <see cref="JwtBearerOptions"/>.
    /// </summary>
    public class PomeloTokenPostConfigureOptions : IPostConfigureOptions<PomeloTokenOptions>
    {
        /// <summary>
        /// Invoked to post configure a JwtBearerOptions instance.
        /// </summary>
        /// <param name="name">The name of the options instance being configured.</param>
        /// <param name="options">The options instance to configure.</param>
        public void PostConfigure(string name, PomeloTokenOptions options)
        {
        }
    }
}