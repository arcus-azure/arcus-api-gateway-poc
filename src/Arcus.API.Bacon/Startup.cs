using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Arcus.API.Bacon.Repositories;
using Arcus.API.Bacon.Repositories.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Arcus.API.Bacon
{
    public class Startup
    {
        private const string ApplicationInsightsInstrumentationKeyName = "ApplicationInsights_InstrumentationKey";

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration of key/value application properties.
        /// </summary>
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });
            services.AddControllers(options => 
            {
                options.ReturnHttpNotAcceptable = true;
                options.RespectBrowserAcceptHeader = true;

                RestrictToJsonContentType(options);
                ConfigureJsonFormatters(options);

            });

            services.AddHealthChecks();
            services.AddHttpCorrelationFromPoc();
            
            services.AddScoped<IBaconRepository, BaconRepository>();

            ConfigureOpenApiGeneration(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandling();
            app.UseHttpCorrelationFromPoc();
            app.UseRouting();
            app.UseRequestTracking();
            
            ExposeOpenApiDocs(app);

            Log.Logger = CreateLoggerConfiguration(app.ApplicationServices).CreateLogger();
        }

        private LoggerConfiguration CreateLoggerConfiguration(IServiceProvider serviceProvider)
        {
            var instrumentationKey = Configuration.GetValue<string>(ApplicationInsightsInstrumentationKeyName);
            
            return new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithVersion()
                .Enrich.WithComponentName("Bacon API")
                .Enrich.WithHttpCorrelationInfo(serviceProvider)
                .WriteTo.Console()
                .WriteTo.AzureApplicationInsightsOnSteroids(instrumentationKey);
        }

        private static void ConfigureOpenApiGeneration(IServiceCollection services)
        {
            var openApiInformation = new OpenApiInfo
            {
                Title = "Arcus - Bacon API",
                Version = "v1"
            };

            services.AddSwaggerGen(swaggerGenerationOptions =>
            {
                swaggerGenerationOptions.SwaggerDoc("v1", openApiInformation);
                swaggerGenerationOptions.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                    "Arcus.API.Bacon.Open-Api.xml"));

                swaggerGenerationOptions.OperationFilter<AddHeaderOperationFilter>("X-Transaction-Id",
                    "Transaction ID is used to correlate multiple operation calls. A new transaction ID will be generated if not specified.",
                    false);
                swaggerGenerationOptions.OperationFilter<AddResponseHeadersFilter>();
            });
        }

        private static void RestrictToJsonContentType(MvcOptions options)
        {
            var allButJsonInputFormatters = options.InputFormatters.Where(formatter => !(formatter is SystemTextJsonInputFormatter));
            foreach (IInputFormatter inputFormatter in allButJsonInputFormatters)
            {
                options.InputFormatters.Remove(inputFormatter);
            }

            // Removing for text/plain, see https://docs.microsoft.com/en-us/aspnet/core/web-api/advanced/formatting?view=aspnetcore-3.0#special-case-formatters
            options.OutputFormatters.RemoveType<StringOutputFormatter>();
        }

        private static void ConfigureJsonFormatters(MvcOptions options)
        {
            var onlyJsonInputFormatters = options.InputFormatters.OfType<SystemTextJsonInputFormatter>();
            foreach (SystemTextJsonInputFormatter inputFormatter in onlyJsonInputFormatters)
            {
                inputFormatter.SerializerOptions.IgnoreNullValues = true;
                inputFormatter.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }

            var onlyJsonOutputFormatters = options.OutputFormatters.OfType<SystemTextJsonOutputFormatter>();
            foreach (SystemTextJsonOutputFormatter outputFormatter in onlyJsonOutputFormatters)
            {
                outputFormatter.SerializerOptions.IgnoreNullValues = true;
                outputFormatter.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            }
        }

        private static void ExposeOpenApiDocs(IApplicationBuilder app)
        {
            app.UseSwagger(swaggerOptions => { swaggerOptions.RouteTemplate = "api/{documentName}/docs.json"; });
            app.UseSwaggerUI(swaggerUiOptions =>
            {
                swaggerUiOptions.SwaggerEndpoint("/api/v1/docs.json", "Arcus - Bacon API");
                swaggerUiOptions.RoutePrefix = "api/docs";
                swaggerUiOptions.DocumentTitle = "Arcus.API.Bacon";
            });
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
