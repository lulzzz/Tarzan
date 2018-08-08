using Cassandra;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using NSwag.AspNetCore;
using System;
using System.Net;
using System.Reflection;
using Tarzan.Nfx.Dashboard.DataAccess;
using Tarzan.Nfx.Dashboard.DataAccess.Cassandra;

namespace Tarzan.Nfx.Dashboard
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
            services.AddMvc();
            services.AddSwagger();

            var keyspace = "testbed";
            var cluster = Cluster.Builder()
                .AddContactPoints(new IPEndPoint(IPAddress.Loopback, 9042))
                .Build();
            var session = cluster.Connect(keyspace);
            var flowsDataAccess = new FlowsDataAccess(session);
            var hostsDataAccess = new HostsDataAccesss(session);
            var servicesDataAccess = new ServicesDataAccesss(session);

            var hostingEnvironment = services[0].ImplementationInstance as IHostingEnvironment;

            services.AddSingleton<ITableDataAccess<Tarzan.Nfx.Model.Flow, Guid>>(flowsDataAccess);
            services.AddSingleton<ITableDataAccess<Tarzan.Nfx.Model.Host, string>>(hostsDataAccess);
            services.AddSingleton<ITableDataAccess<Tarzan.Nfx.Model.Service, string>>(servicesDataAccess);
            services.AddSingleton<ITableDataAccess<Tarzan.Nfx.Model.Capture, Guid>>(new DataAccess.Mock.CapturesDataAccess());

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseSwaggerUi(typeof(Startup).GetTypeInfo().Assembly, settings =>
            {
                settings.GeneratorSettings.DefaultPropertyNameHandling =
                    PropertyNameHandling.CamelCase;
            });

            app.UseMvcWithDefaultRoute();
        }
    }
}
