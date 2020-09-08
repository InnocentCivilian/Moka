using Microsoft.Extensions.DependencyInjection;
using Moka.Sdk.Extenstion;

namespace Moka.Sdk
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddExtensions();
        }
    }
}