using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Cassandra;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NJsonSchema;
using NSwag.AspNetCore;
using Tarzan.UI.Server.DataAccess;

namespace dashboard
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

            var cluster = Cluster.Builder()
                .AddContactPoints(new IPEndPoint(IPAddress.Loopback, 9042))
                .Build();
            var flowsDataAccess = new Tarzan.UI.Server.DataAccess.Cassandra.FlowsDataAccess(cluster, "testbed");
            var hostsDataAccess = new Tarzan.UI.Server.DataAccess.Cassandra.HostsDataAccesss(cluster, "testbed");


            var hostingEnvironment = services[0].ImplementationInstance as IHostingEnvironment;
            services.AddSingleton<IFlowsDataAccess>(flowsDataAccess);
            services.AddSingleton<IHostsDataAccess>(hostsDataAccess);

            services.AddSingleton<ICapturesDataAccess>(new Tarzan.UI.Server.DataAccess.Mock.CapturesDataAccess());

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
