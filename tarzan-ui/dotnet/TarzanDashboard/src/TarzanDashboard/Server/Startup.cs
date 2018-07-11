using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;

namespace TarzanDashboard
{
  /// <summary>
  /// Startup class
  /// </summary>
  public class Startup
  {
    /// <summary>
    /// Configures app the services.
    /// </summary>
    /// <param name="services">The services.</param>
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddMvc();
      // enables cross site requests:
      services.AddCors();

      services.AddSpaStaticFiles(c =>
      {
        c.RootPath = "dist/tarzanDashboard"; // "wwwroot";
      });

      services.AddSwaggerGen(c =>
      {
        c.SwaggerDoc("v1", new Info
        {
          Version = "v1", 
          Title = "TarzanDashboard API",
          Description = "TarzanDashboard ASP.NET Core Web API",
          Contact = new Contact { Name = "Ondrej Rysavy", Email = "rysavy@fit.vutbr.cz", Url = "https://tarzan.fit.vutbr.cz" },
        });
        // Set the comments path for the Swagger JSON and UI.
        var basePath = AppContext.BaseDirectory;
        var xmlPath = Path.Combine(basePath, "TarzanDashboard.xml");
        c.IncludeXmlComments(xmlPath);
      });
    }

    /// <summary>
    /// Configures the application.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <param name="env">The hosting environment</param>
    public void Configure(IApplicationBuilder app, IHostingEnvironment env)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseCors(builder =>
        builder.WithOrigins("http://localhost").AllowAnyHeader());

      app.UseDefaultFiles();
      app.UseStaticFiles();

      // Enable middleware to serve generated Swagger as a JSON endpoint.
      app.UseSwagger();

      // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
      app.UseSwaggerUI(c =>
      {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
      });
      app.UseStaticFiles();
      app.UseSpaStaticFiles();
      app.UseMvc(routes =>
      {
        routes.MapRoute(name: "default", template: "{controller}/{action=index}/{id}");
      });

      app.UseSpa(spa =>
      {
        // To learn more about options for serving an Angular SPA from ASP.NET Core,
        // see https://go.microsoft.com/fwlink/?linkid=864501

        spa.Options.SourcePath = "dist/tarzanDashboard"; //"wwwroot";

        if (env.IsDevelopment())
        {
          //spa.UseProxyToSpaDevelopmentServer("http://localhost:4200");
          spa.UseAngularCliServer(npmScript: "start");
        }
      });
    }
  }
}