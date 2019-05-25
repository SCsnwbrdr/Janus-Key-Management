using JanusKeyManagement;
using KeyVaultExample.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            IServiceProvider provider = services.BuildServiceProvider();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var arrayOfUrls = Configuration.GetSection("AzureMessageQueue:KeyVault:KeyVaultUrls").Get<string[]>();
            var janusConfigSection = Configuration.GetSection("JanusKeyEngine").Get<Dictionary<string, string[]>>();

            services.AddSingleton<MemoryService>();
            services.AddSingleton<IJanusKeyEngine>(new JanusAzureKeyEngine(janusConfigSection));
            services.AddSingleton<IAzureServiceBusService>(sp => new AzureServiceBusService(sp.GetService<MemoryService>(), Configuration.GetValue<string>("AzureMessageQueue:KeyVault:EndPoint"),
                Configuration.GetValue<string>("AzureMessageQueue:KeyVault:QueueName"),
                Configuration.GetValue<string>("AzureMessageQueue:KeyVault:QueueAccessKeyName"), sp.GetService<IJanusKeyEngine>()));
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
