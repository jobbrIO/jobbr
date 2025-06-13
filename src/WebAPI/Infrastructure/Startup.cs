using System;
using Jobbr.Server.WebAPI.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Jobbr.Server.WebAPI.Infrastructure
{
    /// <summary>
    /// Application startup.
    /// </summary>
    public class Startup
    {
        private readonly JobbrWebApiConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Web API configuration.</param>
        public Startup(JobbrWebApiConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Configure application services.
        /// </summary>
        /// <param name="services">Service collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(o =>
            {
                o.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });
            });

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = DefaultJsonOptions.Options.PropertyNamingPolicy;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = DefaultJsonOptions.Options.PropertyNameCaseInsensitive;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = DefaultJsonOptions.Options.DefaultIgnoreCondition;
                });
        }

        /// <summary>
        /// Configure application builder.
        /// </summary>
        /// <param name="app">Application builder.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UsePathBase(new Uri(_configuration.BackendAddress).AbsolutePath);

            app.UseRouting();

            app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}