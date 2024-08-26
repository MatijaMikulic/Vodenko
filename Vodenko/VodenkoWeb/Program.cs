using MessageBroker.Common;
using MessageBroker.Common.Configurations;
using MessageBroker.Common.Producer;
using VodenkoWeb.Hubs;
using VodenkoWeb.Model;
using VodenkoWeb.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using DataAccess.Repository.IRepository;
using DataAccess.Repository;
using OfficeOpenXml;
using ModelProvider.Interfaces;
using Microsoft.Extensions.Configuration;

namespace VodenkoWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddControllers(); 

            builder.Services.AddHttpClient();

            builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            builder.Services.AddDbContext<ApplicationDbContext>(
                options =>options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddSignalR();

            builder.Services.AddDirectoryBrowser();

            // Adding rabbitMq service
            builder.Services.Configure<RabbitMqConfiguration>(builder.Configuration.GetSection("RabbitMqConfiguration"));
            builder.Services.Configure<RabbitMqModelSettings>(builder.Configuration.GetSection("RabbitMqModelSettings"));
            builder.Services.Configure<ExternalUrls>(builder.Configuration.GetSection("ExternalUrls"));

            builder.Services.AddTransient<IRabbitMqService, RabbitMqService>();
            builder.Services.AddTransient<IProducerConsumer, RabbitMqProducerConsumer>();
            builder.Services.AddScoped<ParametersService, ParametersService>();

            builder.Services.AddSingleton<RecordService>();

            builder.Services.AddSingleton(_ => {
                var buffer = new DynamicDataBuffer(200);
                return buffer;
            });

            builder.Services.AddSingleton(_ => {
                var logBuffer = new LogBuffer(50);
                return logBuffer;
            });

            builder.Services.AddSingleton<IModelProvider, ModelProvider.ModelProvider>();
            builder.Services.AddScoped<INotificationService,NotificationService>();

            builder.Services.AddHostedService<DynamicDataFetchService>();
            builder.Services.AddHostedService<LogMonitoringService>();
            builder.Services.AddHostedService<CachingService>();

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    OnPrepareResponse = ctx =>
            //    {
            //        var extension = Path.GetExtension(ctx.File.PhysicalPath);
            //        if (extension.Equals(".glb", StringComparison.OrdinalIgnoreCase))
            //        {
            //            ctx.Context.Response.ContentType = "model/gltf-binary";
            //        }
            //    }
            //});
            StaticFileOptions options = new StaticFileOptions { ContentTypeProvider = new FileExtensionContentTypeProvider() };
            options.ServeUnknownFileTypes = true;
            app.UseStaticFiles(options);

            //app.UseFileServer(enableDirectoryBrowsing: true);

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllers();

            app.MapHub<ChartHub>(ChartHub.Url);
            app.MapHub<LogHub>(LogHub.Url);
            app.MapHub<CacheHub>(CacheHub.Url);
            app.MapHub<NotificationHub>(NotificationHub.Url);   

            app.Run();
        }
    }
}