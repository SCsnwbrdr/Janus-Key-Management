using KeyVaultExample.Service;
using KeyVaultQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace KeyVaultExample
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            var arrayOfUrls = Configuration.GetSection("AzureMessageQueue:KeyVault:KeyVaultUrls").Get<string[]>();
            services.AddSingleton<MemoryService>();
            services.AddScoped<AzureServiceBusService>();
            services.AddSingleton<KeyVaultMessageQueue>(new KeyVaultMessageQueue(Configuration.GetValue<string>("AzureMessageQueue:KeyVault:EndPoint"),
                Configuration.GetValue<string>("AzureMessageQueue:KeyVault:QueueName"),
                Configuration.GetValue<string>("AzureMessageQueue:KeyVault:QueueAccessKeyName"), arrayOfUrls
                ));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
