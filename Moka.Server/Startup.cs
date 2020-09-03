using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moka.Server.Data;
using Moka.Server.Service;
using Grpc.Core;
using Moka.Server.Events;
using Moka.Server.Helper;
using Moka.Server.Manager;

namespace Moka.Server
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            // requires using Microsoft.Extensions.Options
            services.AddHttpContextAccessor();
 
            // services.AddAuthorization(options =>
            // {
            //     options.AddPolicy("protectedScope", policy =>
            //     {
            //         policy.RequireClaim("scope", "grpc_protected_scope");
            //     });
            // });

            services.Configure<MokaDataBaseSettings>(Configuration.GetSection(nameof(MokaDataBaseSettings)));
            services.AddSingleton<UserEvents>();
            services.AddSingleton<MessageEvents>();
            services.AddSingleton<OnlineUsersManager>();

            services.AddSingleton<IMokaDataBaseSettings>(sp =>
                sp.GetRequiredService<IOptions<MokaDataBaseSettings>>().Value);

            services.AddScoped<IUserService,UserService>();
            services.AddScoped<IMessageService,MessageService>();

            services.AddControllers();
            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });
                
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // app.UseAuthentication();
            // app.UseAuthorization();
            // app.UseMiddleware<JwtMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); 
                endpoints.MapGrpcService<MokaMessageService>();
            });
            
        }
    }
}