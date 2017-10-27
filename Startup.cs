using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using ViennaFeedback.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace ViennaFeedback
{
    public class Startup
    {

         public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("config.json")
                .AddJsonFile("appsettings.json")
                .Build();
        }
        // public Startup(IConfiguration configuration)
        // {
        //     Configuration = configuration;
        // }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddScoped<ClientIPCheckilter>();
            services.AddMvc(config => {
                var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
                config.Filters.Add(new AuthorizeFilter(policy));
            });

            //services.AddMvc();

            services.AddDbContext<MvcFeedbackContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("ClientTelemetryDb")));

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                // Configure the OWIN pipeline to use cookie auth.
                .AddCookie()
                // Configure the OWIN pipeline to use OpenID Connect auth.
                .AddOpenIdConnect(option =>
                {
                    option.ClientId = Configuration["AzureAD:ClientId"];
                    option.Authority = String.Format(Configuration["AzureAd:AadInstance"], Configuration["AzureAd:Tenant"]);
                    option.SignedOutRedirectUri = Configuration["AzureAd:PostLogoutRedirectUri"];
                    option.Events = new OpenIdConnectEvents
                    {
                        OnRemoteFailure = OnAuthenticationFailed,
                    };
                });

            // Add Authorization
            services.AddAuthorization(
                options => {
                    //options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("http://schemas.microsoft.com/identity/claims/tenantid", "72f988bf-86f1-41af-91ab-2d7cd011db47"));
                    options.AddPolicy("EmployeeOnly", policy => policy.RequireClaim("http://schemas.microsoft.com/identity/claims/tenantid", "72f988bf-86f1-41af-91ab-2d7cd011db47"));
                }
            );
        }

        private Task OnAuthenticationFailed(RemoteFailureContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
            return Task.FromResult(0);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();
            // Add the console logger.
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            
            app.UseStaticFiles();
            //app.UseIdentity();
            
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
