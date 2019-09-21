﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace EmployeeManagementSystem
{
    public class Startup
    {
        private IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            //adding services for Identity Services
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                //Here we acan change default password options
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
                //Sice we use Entity frame work for Database we use below methods
                    .AddEntityFrameworkStores<AppDbContext>();
            ///2nd way of configuring Pasword Options
            //services.Configure<IdentityOptions>(options=>
            //{
            //    //Here we acan change default password options
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = false;
            //});

            services.AddDbContextPool<AppDbContext>(options => options.UseSqlServer(_config.GetConnectionString("EmployeeDbConnection")));


           services.AddMvc(options=>
           {
               var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
               options.Filters.Add(new AuthorizeFilter(policy));
           }).AddXmlSerializerFormatters();
            //services.AddSingleton<IEmployeeRepository, MockEmployeeRepository>();
            services.AddScoped<IEmployeeRepository, SQLEmployeeRepository>();
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
               
                //If Exception are thown then  we will handle it using
                //Exception is passed to the Error controller
                app.UseExceptionHandler("/Error");

                //Use Redirect by Creating an Error controller and the response status
                ///404 or 500 to {0} parameters
                app.UseStatusCodePagesWithReExecute("/Error/{0}");
            }
            ///Add the Authentication middele ware In the Application request processing pipeline 
            ///ie configure method in startup class we need to call method
            app.UseAuthentication();
            
            app.UseStaticFiles();
            // app.UseMvcWithDefaultRoute();
            // app.UseMvc(routes=> routes.MapRoute("default", "{controller=home}/{action=index}/{id?}"));
            app.UseMvc();
            

           
        }
    }
}
