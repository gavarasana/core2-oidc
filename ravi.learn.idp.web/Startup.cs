﻿using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ravi.learn.idp.web.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ravi.learn.idp.web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Add framework services.
            services.AddMvc();

            // register an IHttpContextAccessor so we can access the current
            // HttpContext in services by injecting it
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // register an IImageGalleryHttpClient
            services.AddScoped<IImageGalleryHttpClient, ImageGalleryHttpClient>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;


                })
               .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options => {
                   options.AccessDeniedPath = "/Authorization/AccessDenied";
               })
               .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
               {
                   options.Authority = "https://localhost:44313";
                   options.ClientId = "ImageGallery";
                   options.ClientSecret = "secret";
                   options.ResponseType = "code id_token";
                   options.Scope.Add("openid");
                   options.Scope.Add("profile");
                   options.Scope.Add("address");
                   options.Scope.Add("roles");
                   options.Scope.Add("Country");
                   options.Scope.Add("SubscriptionLevel");
                   options.Scope.Add("imagegalleryapi");
                   options.Scope.Add("offline_access");
                   options.SaveTokens = true;
                   options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                   options.GetClaimsFromUserInfoEndpoint = true;
                   options.ClaimActions.Remove("amr");
                   options.ClaimActions.DeleteClaim("idp");
                   options.ClaimActions.MapUniqueJsonKey("role", "role");
                   options.ClaimActions.MapUniqueJsonKey("subscriptionlevel", "subscriptionlevel");
                   options.ClaimActions.MapUniqueJsonKey("country", "country");
                   
                   options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                   {
                       NameClaimType = JwtClaimTypes.GivenName,
                       RoleClaimType = JwtClaimTypes.Role
                   };                  
                   
               });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("OrderFrame", policyBuilder =>
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.RequireClaim("country", "be");
                    policyBuilder.RequireClaim("subscriptionlevel", "paiduser");
                });
            });

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
                app.UseExceptionHandler("/Shared/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();


            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
