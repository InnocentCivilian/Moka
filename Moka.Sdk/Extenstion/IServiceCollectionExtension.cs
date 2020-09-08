using System;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moka.Sdk.SqlLite;

namespace Moka.Sdk.Extenstion
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddExtensions(this IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });
            services.AddTransient<ConsoleMenu>();

            services.AddSingleton<IDbConnection>(new DbConnection());
            var ar  = Environment.GetEnvironmentVariable("USER");
            
            var me = new Me(
                new User
                {
                    Id = Guid.Empty.ToString(),
                    Nickname = ar + "name",
                    Username = ar
                },
                EnvConsts.PASSWORD
            );
            services.AddSingleton<IMe>(me);
            
            return services;
        }
    }
}