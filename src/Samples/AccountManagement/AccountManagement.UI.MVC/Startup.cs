﻿using Composable.DependencyInjection;
using Composable.Messaging.Buses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AccountManagement.UI.MVC
{
    public class Startup
    {
        ITestingEndpointHost _host;
        IEndpoint _clientEndpoint;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            _host = EndpointHost.Testing.CreateWithClientEndpoint(DependencyInjectionContainer.Create);
            new AccountManagementServerDomainBootstrapper().RegisterWith(_host);
            _clientEndpoint = _host.RegisterClientEndpoint();
            _host.Start();
            services.AddScoped(_ => _clientEndpoint.ServiceLocator.Resolve<IServiceBusSession>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.Use(async (context, next) => await _clientEndpoint.ExecuteRequestAsync(async () => await next.Invoke()));

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
