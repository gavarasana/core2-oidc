﻿using ImageGallery.API.Entities;
using ImageGallery.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Model = ravi.learn.idp.model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.AccessTokenValidation;
using ravi.learn.idp.api.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ravi.learn.idp.api
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = "https://localhost:44313";
                        options.ApiName = "imagegalleryapi";
                        options.ApiSecret = "secret";
                    });

            services.AddAuthorization(authorizationOptions => 
            {
                authorizationOptions.AddPolicy("MustOwnImage", policyBuilder => 
                {
                    policyBuilder.RequireAuthenticatedUser();
                    policyBuilder.AddRequirements(new MustOwnImageRequirement());

                });
            });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            var connectionString = Configuration["ConnectionStrings:imageGalleryDBConnectionString"];
            services.AddDbContext<GalleryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<IGalleryRepository, GalleryRepository>();
            services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();
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
                app.UseHsts();

                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        // ensure generic 500 status code on fault.
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
            }

            AutoMapper.Mapper.Initialize(cfg =>
            {
                // Map from Image (entity) to Image, and back
                cfg.CreateMap<Image, Model.Image>().ReverseMap();

                // Map from ImageForCreation to Image
                // Ignore properties that shouldn't be mapped
                cfg.CreateMap<Model.ImageForCreation, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());

                // Map from ImageForUpdate to Image
                // ignore properties that shouldn't be mapped
                cfg.CreateMap<Model.ImageForUpdate, Image>()
                    .ForMember(m => m.FileName, options => options.Ignore())
                    .ForMember(m => m.Id, options => options.Ignore())
                    .ForMember(m => m.OwnerId, options => options.Ignore());
            });

            AutoMapper.Mapper.AssertConfigurationIsValid();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
