﻿using MileAPI.DataServices;
using MileAPI.Interfaces;
using MileDALibrary.DataRepository;
using MileDALibrary.Interfaces;
using MileDALibrary.Models;
using CorePush.Apple;
using CorePush.Google;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace MileAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<DBSettings>(Configuration.GetSection("DBSettings"));
            services.Configure<BlobConfig>(Configuration.GetSection("BlobConfig"));
            services.AddHttpClient<FcmSender>();
            services.AddHttpClient<ApnSender>();

            var appSettingsSection = Configuration.GetSection("FcmNotification");
            services.Configure<FcmNotificationSetting>(appSettingsSection);

            ConfigureSwagger(services);
            ConfigureHealthCheck(services);
            RegisterDependencies(services);
            services.AddCors();
        }

        private void ConfigureHealthCheck(IServiceCollection services)
        {
            services.AddHealthChecks();
        }

        private void ConfigureSwagger(IServiceCollection services)
        {
            //Swagger Setup
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "MILE", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("../swagger/v1/swagger.json", "MILE REST APIs");
                //c.RoutePrefix = string.Empty;
            });
            //app.UseSwaggerUI();

            app.UseHealthChecks("/health");

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
            });
        }

        private void RegisterDependencies(IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddSingleton<IBankDetailsService, BankDetailsService>();
            services.AddSingleton<IBankDetailsRepository, BankDetailsRepository>();
            services.AddTransient<INotificationService, NotificationService>();
        }
    }
}
