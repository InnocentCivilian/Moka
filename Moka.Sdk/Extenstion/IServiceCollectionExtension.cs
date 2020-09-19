using System;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moka.Sdk.Entity;
using Moka.Sdk.Service;
using Moka.Sdk.SqlLite;
using Moka.SharedKernel.Encryption;
using Moka.SharedKernel.Extenstion;

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

            services.AddSingleton(new MokaClientContext(EnvConsts.DEFAULTDBFILE));
            var ar = Environment.GetEnvironmentVariable("USER");
            var me = new Me(
                new User
                {
                    Id = Guid.Empty.ToString(),
                    Nickname = ar + "name",
                    Username = ar
                },
                EnvConsts.PASSWORD
            );
            services.AddSingleton(me);
            services.AddKernelExtensions(me.User.Username);
            
            services.AddSingleton<IAsymmetricEncryption>(new RSAEncryption(ar));
            services.AddSingleton<ISymmetricEncryption>(new AesEncryption());
            services.AddSingleton<IHybridEncryption, HybridEncryption>();

            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IUserService, UserService>();

            services.AddSingleton<IMeService, MeService>();

            return services;
        }
    }
}