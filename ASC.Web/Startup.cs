using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using ElCamino.AspNetCore.Identity.AzureTable.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using ASC.Web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ASC.Web.Configuration;
using ASC.Web.Models;
using ASC.DataAccess;
using ASC.DataAccess.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using ASC.Web.Services;
using Microsoft.AspNetCore.Authentication.Google;
using System.Reflection;
using ASC.Business.Interfaces;
using ASC.Business;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.Caching.Redis;
using Microsoft.Extensions.Logging;
using ASC.Web.Logger;
using ASC.Web.Filters;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using ASC.Web.ServiceHub;

namespace ASC.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Elcamino Azure Table Identity services.
            services.AddIdentity<ApplicationUser, ElCamino.AspNetCore.Identity.AzureTable.Model.IdentityRole>((options) =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddAzureTableStores<ApplicationDbContext>(new Func<IdentityConfiguration>(() =>
            {
                IdentityConfiguration idconfig = new IdentityConfiguration();
                idconfig.TablePrefix = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration: TablePrefix").Value;
                idconfig.StorageConnectionString = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration:StorageConnectionString").Value;
                idconfig.LocationMode = Configuration.GetSection("IdentityAzureTable:IdentityConfiguration: LocationMode").Value;
                return idconfig;
            }))
            .AddDefaultTokenProviders()
            .CreateAzureTablesIfNotExists<ApplicationDbContext>();

            services.AddAuthentication().AddGoogle(options =>
            {
                options.ClientId = Configuration["Google:Identity:ClientId"];
                options.ClientSecret = Configuration["Google:Identity:ClientSecret"];
            });

            //services.AddDistributedMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = Configuration.GetSection("CacheSettings:CacheConnectionString").Value;
                options.InstanceName = Configuration.GetSection("CacheSettings:CacheInstance").Value;
            });
            services.AddSession();
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });
            services.AddControllersWithViews(o => {
                o.Filters.Add(typeof(CustomExceptionFilter));
                o.EnableEndpointRouting = false;
                })
                .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
            services.AddRazorPages();
            services.AddOptions();
            services.Configure<ApplicationSettings>(this.Configuration.GetSection("AppSettings"));
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<IIdentitySeed, IdentitySeed>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUnitOfWork>(p => new UnitOfWork(Configuration.GetSection("ConnectionStrings:DefaultConnection").Value));
            services.AddScoped<IMasterDataOperations, MasterDataOperations>();
            services.AddAutoMapper(typeof(Startup));
            services.AddSingleton<IMasterDataCacheOperations, MasterDataCacheOperations>();
            services.AddScoped<ILogDataOperations, LogDataOperations>();
            services.AddScoped<CustomExceptionFilter>();
            services.AddSingleton<INavigationCacheOperations, NavigationCacheOperations>();
            services.AddScoped<IServiceRequestOperations, ServiceRequestOperations>();
            services.AddScoped<IServiceRequestMessageOperations, ServiceRequestMessageOperations>();

            //Below code does not seem to be working..need to fix later
            //var assembly = typeof(ASC.Utilities.Navigation.LeftNavigationViewComponent).GetTypeInfo().Assembly;
            //var embeddedFileProvider = new EmbeddedFileProvider(assembly, "ASC.Utilities");
            //services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            //{
            //    options.FileProviders.Add(embeddedFileProvider);
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IUnitOfWork unitOfWork, ILoggerFactory loggerFactory,
                                IMasterDataCacheOperations masterDataCacheOperations, ILogDataOperations logDataOperations,
                                INavigationCacheOperations navigationCacheOperations)
        {
            var configBuilder = new ConfigurationBuilder()
                                .SetBasePath(env.ContentRootPath)
                                .AddJsonFile("appsettings.json", false, true)
                                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", false, true);
            configBuilder.AddEnvironmentVariables();
            Configuration = configBuilder.Build();

            loggerFactory.AddAzureTableStorageLog(logDataOperations, (categoryName, logLevel) => !categoryName.Contains("Microsoft") && logLevel >= LogLevel.Information);

            if (env.IsDevelopment())
            {
                /*Handle exceptions using Global Exception filter*/

                //app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseSession();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();


            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapAreaControllerRoute(name: "areas", areaName: "areas", "{area:exists}/{controller=Home}/{action=Index}");
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //    endpoints.MapRazorPages();
            //});
            app.UseMvc(routes =>
            {
                routes.MapRoute(name: "areaRoute",
                template: "{area:exists}/{controller=Home}/{action=Index}");

                routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.UseWebSockets();
            app.UseSignalR(routes =>
            {
                routes.MapHub<ServiceMessagesHub>("/ServiceMessagesHub");
            });

            var models = Assembly.Load(new AssemblyName("ASC.Models")).GetTypes().Where(t => t.Namespace.Equals("ASC.Models.Models"));
            foreach(var model in models)
            {
                var repositoryInstance = Activator.CreateInstance(typeof(Repository<>).MakeGenericType(model), unitOfWork);
                MethodInfo methodInfo = typeof(Repository<>).MakeGenericType(model).GetMethod("CreateTableAsync");
                methodInfo.Invoke(repositoryInstance, new object[0]);
            }

            masterDataCacheOperations.CreateMasterDataCacheAsync();
            navigationCacheOperations.CreateNavigationMenuCacheAsync();
        }
    }
}
