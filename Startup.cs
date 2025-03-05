using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using webapi.SqlServer;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System;

namespace webapi
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
            services.AddControllers(); 
            var rawString = Configuration.GetConnectionString("DataStore");

            var connectionStrings = rawString.Split("---");
            var dataStoreConnectionString = connectionStrings[0];
            var cacheConnectionString = connectionStrings[1];

            // Register the DbContext with the connection string from the configuration
            services.AddDbContext<StickersContext>(options =>
                options.UseSqlServer(dataStoreConnectionString));
            // Register the Redis cache service
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheConnectionString;
                options.InstanceName = "SampleInstance";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}