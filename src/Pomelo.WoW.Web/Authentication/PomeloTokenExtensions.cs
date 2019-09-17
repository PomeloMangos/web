using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Pomelo.WoW.Web.Authentication;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class JwtBearerExtensions
    {
        public static AuthenticationBuilder AddPomeloToken(
            this AuthenticationBuilder builder)
            => builder.AddPomeloToken(PomeloTokenHandler.Scheme, null, _ => { });

        public static AuthenticationBuilder AddPomeloToken(
            this AuthenticationBuilder builder, 
            string authenticationScheme, 
            string displayName,
            Action<PomeloTokenOptions> configureOptions)
        {
            builder
                .Services
                .TryAddEnumerable(
                    ServiceDescriptor.Singleton<IPostConfigureOptions<PomeloTokenOptions>, PomeloTokenPostConfigureOptions>());

            return builder.AddScheme<PomeloTokenOptions, PomeloTokenHandler>(
                authenticationScheme, 
                displayName, 
                configureOptions);
        }
    }
}