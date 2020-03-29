using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grace.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestMediatr.Commands;

namespace TestMediatr
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var assembly = typeof(Startup).Assembly;
            
            services.AddMediatR(assembly);
            services.AddHandlers();
            
        }

        public void ConfigureContainer(IInjectionScope injection)
        {
            injection.Configure(c =>
            {
                c.ExportDecorator<IRequestHandler<PingCommand, string>>(
                    request => new PingCommand.AuditDecorator<PingCommand, string>(request));
                c.ExportDecorator<IRequestHandler<PingCommand, string>>(
                    request => new PingCommand.DatabaseDecorator<PingCommand, string>(request));
                c.ExportDecorator<IRequestHandler<PingCommand, string>>(
                    request => new PingCommand.ExtenderDecorator(request));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}
