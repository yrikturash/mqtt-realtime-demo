using System;
using API.Infrastructure;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard.Management.v2;
using Hangfire.JobsLogger;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API
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
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseMemoryStorage(new MemoryStorageOptions { FetchNextJobTimeout = TimeSpan.FromHours(24) }) //https://github.com/HangfireIO/Hangfire/issues/1197
                .UseJobsLogger()
                .UseConsole()
                .UseManagementPages(typeof(Startup).Assembly)
                //.UseMissionControl(typeof(Startup).Assembly)
            );


            // Add the processing server as IHostedService
            services.AddHangfireServer();
            services.AddControllers();
            services.AddHealthChecks();
            services.AddOpenApiDocument();

            services.AddTransient<GpsTrackSender>();


            services.Configure<MessageBusOptions>(Configuration.GetSection("MessageBus"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient backgroundJobs)
        {
            app.UsePathBase("/gps-track-simulation");

            app.Use((context, next) =>
            {
                context.Request.PathBase = "/gps-track-simulation";
                return next();
            });

            // ref: https://github.com/aspnet/Docs/issues/2384
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseHsts();
            app.UseHttpsRedirection();

            app.UseMiddleware<RequestLoggingMiddleware>();

            app.UseRouting();
            app.UseOpenApi(configure => configure.PostProcess = (document, _) => document.Schemes = new[] { NSwag.OpenApiSchema.Https });
            app.UseSwaggerUi3(settings =>
            {
            });

            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions
                {
                    Authorization = new[] { new HangfireSkipAuthorizationFilter() }
                });
            });
        }
    }
}
