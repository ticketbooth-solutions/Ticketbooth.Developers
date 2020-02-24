using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Ticketbooth.ApiDemo.Services;
using Ticketbooth.ApiDemo.Services.Background;

namespace Ticketbooth.ApiDemo
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<WalletService>();

            services.AddSignalR();

            services.AddHostedService<FullNodeMonitor>();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
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
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // wwwroot embedded into assembly to access from node
            var currentAssembly = Assembly.GetExecutingAssembly();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new EmbeddedFileProvider(currentAssembly, $"{currentAssembly.GetName().Name}.wwwroot")
            });

            app.UseSignalR(builder =>
            {
                builder.MapHub<DemoHub>("/demoHub");
            });

            app.UseMvc();
        }
    }
}
