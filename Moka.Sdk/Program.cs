using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moka.Sdk.Entity;
using Moka.Sdk.Extenstion;
using Moka.SharedKernel.Encryption;
using Moka.SharedKernel.Tests.Encryption;

namespace Moka.Sdk
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = new ServiceCollection()
                .AddExtensions()
                .BuildServiceProvider();
            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("CLI Started...");
            var menu = serviceProvider.GetService<ConsoleMenu>();
            var me = serviceProvider.GetService<Me>();
            Console.WriteLine(new RSAEncryption(me.User.Username));
            // Console.WriteLine(RSAHandler.GetOrGenerateKeyPair(me.User.Username));//todo change to guid
            await menu.ShowMenu();
        }
    }
}