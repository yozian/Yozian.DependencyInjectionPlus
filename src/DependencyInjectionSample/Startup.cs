using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DependencyInjectionSample.Services;
using DependencyInjectionSample.Services.Impl;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Yozian.DependencyInjectionPlus;
namespace DependencyInjectionSample
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            // 1. register services
            //services.RegisterServices();

            // 2. register services only in those assmbly name start with "DependencyInjectionSample"
            //services.RegisterServices("DependencyInjectionSample");


            // 3. register services and has some condition
            //services.RegisterServices("", type =>
            //{
            //    return type.Name.Contains("Service");
            //});


            // 4. regiter services for specify assembly
            services.RegisterServicesOfAssembly(new { }.GetType().Assembly);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                var demoService = context.RequestServices.GetService<DemoService>();
                var workService = context.RequestServices.GetService<WorkService>();
                var workServiceByInterface = context.RequestServices.GetService<IWorker>();

                demoService.ShowTime();

                workService.DoWork();

                workServiceByInterface.DoWork();


                await context.Response.WriteAsync("Hello World!");
            });




        }
    }
}
