using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moka.Server.Data;
using Moka.Server.Service;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Moka.Server.Events;
using Moka.Server.Helper;
using Moka.Server.Manager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Moka.Server.Auth;

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

            services.Configure<MokaDataBaseSettings>(Configuration.GetSection(nameof(MokaDataBaseSettings)));
            services.AddSingleton<UserEvents>();
            services.AddSingleton<MessageEvents>();
            services.AddSingleton<OnlineUsersManager>();

            services.AddSingleton<IMokaDataBaseSettings>(sp =>
                sp.GetRequiredService<IOptions<MokaDataBaseSettings>>().Value);

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMessageService, MessageService>();

            services.AddControllers();

            services.AddGrpc(options => { options.EnableDetailedErrors = true; });

            var tokenKey = Configuration.GetValue<string>("TokenKey");
            var key = Encoding.ASCII.GetBytes(tokenKey);
            var myIssuer = "http://hmnhp.ir";
            var myAudience = "http://hmnhpaudit.ir";
            services.AddSingleton<IJWTAuthenticationManager>(new JWTAuthenticationManager(tokenKey,myIssuer,myAudience));

            services.AddAuthentication("Basic")
                .AddScheme<BasicAuthenticationOptions, CustomAuthenticationHandler>("Basic", null);

            services.AddSingleton<ICustomAuthenticationManager>(new CustomAuthenticationManager(tokenKey,myIssuer,myAudience));
            services.AddHttpContextAccessor();


            // services.AddAuthentication(x =>
            //     {
            //         x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //         x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //     })
            //     .AddJwtBearer(x =>
            //     {
            //         x.RequireHttpsMetadata = false;
            //         x.SaveToken = true;
            //         x.TokenValidationParameters = new TokenValidationParameters
            //         {
            //             ValidateIssuerSigningKey = true,
            //             IssuerSigningKey = new SymmetricSecurityKey(key),
            //             ValidateIssuer = false,
            //             ValidateAudience = false
            //         };
            //     });

            // services.AddAuthorization(options =>
            // {
            //     options.AddPolicy(JwtBearerDefaults.AuthenticationScheme, policy =>
            //     {
            //         policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme);
            //         policy.RequireClaim(ClaimTypes.Name);
            //     });
            // });
            // services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            //     .AddJwtBearer(options =>
            //     {
            //         options.TokenValidationParameters =
            //             new TokenValidationParameters
            //             {
            //                 ValidateAudience = false,
            //                 ValidateIssuer = false,
            //                 ValidateActor = false,
            //                 ValidateLifetime = true,
            //                 IssuerSigningKey = SecurityKey
            //             };
            //     });
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

            app.UseAuthentication();
            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<MokaMessageService>();
            });
        }
    }
}