using System;
using System.IO;
using System.Reflection;
using Fundo.Application;
using Fundo.Applications.WebApi.Configuration;
using Fundo.Applications.WebApi.Constants;
using Fundo.Applications.WebApi.Middleware;
using Fundo.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Fundo.Applications.WebApi
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly CorsSettings corsSettings;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
            corsSettings = GetCorsSettings(configuration);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplication();
            services.AddInfrastructure(configuration);
            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy(corsSettings.PolicyName, policy =>
                {
                    policy
                        .WithOrigins(corsSettings.AllowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                var xmlPath = GetXmlCommentsPath();
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

            app.UseRouting();
            if (env.IsDevelopment())
            {
                // MVP: CORS is enabled only for local frontend work; allowed origins are configuration-driven.
                app.UseCors(corsSettings.PolicyName);
            }

            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }

        private static string GetXmlCommentsPath()
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            return Path.Combine(AppContext.BaseDirectory, xmlFile);
        }

        private static CorsSettings GetCorsSettings(IConfiguration configuration)
        {
            var settings = configuration
                .GetSection(CorsSettings.SectionName)
                .Get<CorsSettings>();

            if (settings is null)
            {
                throw new InvalidOperationException(ErrorMessages.MissingCorsConfiguration);
            }

            if (string.IsNullOrWhiteSpace(settings.PolicyName))
            {
                throw new InvalidOperationException(ErrorMessages.CorsPolicyNameRequired);
            }

            if (settings.AllowedOrigins.Length == 0)
            {
                throw new InvalidOperationException(ErrorMessages.CorsAllowedOriginsRequired);
            }

            return settings;
        }
    }
}
