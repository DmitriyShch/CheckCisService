using CheckCisService.Config;
using CheckCisService.Helpers;
using CheckCisService.Repositories;
using CheckCisService.Services;
using Microsoft.AspNetCore.Authentication;
using Serilog;

namespace CheckCisService
{
    public class Program
    {
        private const string SETTINGS_FILE = "appsettings.json";
        private const string MDLP_CONFIG_SECTION = "Mdlp";

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            AddConfig(builder.Services);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthorization();
            builder.Services
                .AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions,
                BasicAuthenticationHandler>("BasicAuthentication", null);

            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss.ms} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File("Logs/log-.txt",
                    outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.ms} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7) // храним 7 дней
                .CreateLogger();

            Log.Logger = logger;

            // Подключаем Serilog как логгер
            builder.Host.UseSerilog();

            //var app = builder.Build();

            builder.Services.AddTransient<MdlpCashRegHelper>();
            builder.Services.AddSingleton<MarkingApiService>();
            builder.Services.AddTransient<MarkingOfflineService>();
            builder.Services.AddTransient<MarkingOnlineService>();
            builder.Services.AddTransient<MdlpCheckCisLogService>();
            builder.Services.AddTransient<MdlpCheckCisLogRepository>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers()
                .RequireAuthorization();

            app.Run();
        }

        private static void AddConfig(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(SETTINGS_FILE, optional: true)
                .Build();
            services.Configure<MdlpConfig>(configuration.GetSection(MDLP_CONFIG_SECTION));
        }
    }
}
