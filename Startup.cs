using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace WebAppSSO
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
           services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            /***** Begin SSO Authentication *****/

            services.AddAuthentication(sharedOptions =>
                {
                    sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                //Add Open Id connect extension handler and provide the following values to authentication the user to Common Service SSO login
                .AddOpenIdConnect(options =>
                {
                    options.Authority = Configuration.GetValue<string>("CSAzureAdConfigSettings:Instance") +
                                        Configuration.GetValue<string>("CSAzureAdConfigSettings:TenantId");
                    options.ClientId = Configuration.GetValue<string>("CSAzureAdConfigSettings:ClientId");
                    options.CallbackPath = Configuration.GetValue<string>("CSAzureAdConfigSettings:CallbackPath");
                })
                .AddCookie();

            /***** End SSO Authentication *****/

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();
            }

            //Add this UseHttpsRedirection middleware for redirecting HTTP Requests to HTTPS
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            /***** Begin SSO Authentication *****/
            app.UseAuthentication();
            /***** End SSO Authentication *****/

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
