using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moka.SharedKernel.Encryption;

namespace Moka.SharedKernel.Extenstion
{
    public static class IServiceCollectionExtension
    {
        public static IServiceCollection AddKernelExtensions(this IServiceCollection services,string owner)
        {
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });

            services.AddSingleton<ISymmetricEncryption,AesEncryption>();
            services.AddSingleton<IAsymmetricEncryption>(new RSAEncryption(owner));
            services.AddSingleton<IHybridEncryption,HybridEncryption>();
            return services;
        }
    }
}