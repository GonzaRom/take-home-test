using System;
using System.IO;
using System.Reflection;
using System.Threading;
using Fundo.Application;
using Fundo.Applications.WebApi.Configuration;
using Fundo.Applications.WebApi.Constants;
using Fundo.Applications.WebApi.Middleware;
using Fundo.Infrastructure;
using Fundo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fundo.Applications.WebApi
{
    public class Startup
    {
        private readonly IConfiguration configuration;
        private readonly CorsSettings corsSettings;
        private readonly DatabaseSettings databaseSettings;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
            corsSettings = GetCorsSettings(configuration);
            databaseSettings = GetDatabaseSettings(configuration);
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            ApplyDatabaseMigrationsIfEnabled(app, env, logger);

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

        private void ApplyDatabaseMigrationsIfEnabled(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (!env.IsDevelopment() || !databaseSettings.AutoMigrate)
            {
                return;
            }

            // Automatic migrations are intended only for local Docker / Development review.
            // Production deployments should run migrations explicitly as part of deployment, not from app startup.
            ValidateDatabaseMigrationSettings(databaseSettings);

            var migrationRetryDelay = TimeSpan.FromSeconds(databaseSettings.MigrationRetryDelaySeconds);

            for (var attempt = 1; attempt <= databaseSettings.MigrationMaxAttempts; attempt++)
            {
                try
                {
                    logger.LogInformation(
                        "Applying EF Core database migrations. Attempt {Attempt} of {MaxAttempts}.",
                        attempt,
                        databaseSettings.MigrationMaxAttempts);

                    using var scope = app.ApplicationServices.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<LoanDbContext>();
                    dbContext.Database.Migrate();

                    logger.LogInformation("EF Core database migrations applied successfully.");
                    return;
                }
                catch (Exception ex) when (attempt < databaseSettings.MigrationMaxAttempts)
                {
                    logger.LogWarning(
                        ex,
                        "EF Core database migration attempt {Attempt} of {MaxAttempts} failed. Retrying in {RetryDelaySeconds} seconds.",
                        attempt,
                        databaseSettings.MigrationMaxAttempts,
                        migrationRetryDelay.TotalSeconds);
                    Thread.Sleep(migrationRetryDelay);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "EF Core database migrations failed after {MaxAttempts} attempts. API startup will stop.",
                        databaseSettings.MigrationMaxAttempts);
                    throw;
                }
            }
        }

        private static void ValidateDatabaseMigrationSettings(DatabaseSettings settings)
        {
            if (settings.MigrationMaxAttempts <= 0)
            {
                throw new InvalidOperationException($"{DatabaseSettings.SectionName}:{nameof(DatabaseSettings.MigrationMaxAttempts)} must be greater than zero.");
            }

            if (settings.MigrationRetryDelaySeconds <= 0)
            {
                throw new InvalidOperationException($"{DatabaseSettings.SectionName}:{nameof(DatabaseSettings.MigrationRetryDelaySeconds)} must be greater than zero.");
            }
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

        private static DatabaseSettings GetDatabaseSettings(IConfiguration configuration)
        {
            return configuration
                .GetSection(DatabaseSettings.SectionName)
                .Get<DatabaseSettings>() ?? new DatabaseSettings();
        }
    }
}
