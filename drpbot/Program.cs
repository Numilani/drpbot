using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;
using drpbot;
using drpbot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json")
    .Build();

SetupLogging(config.GetConnectionString("DefaultConnection"));

try
{
    Log.Information("Starting...");
    
    using IHost host = Host.CreateDefaultBuilder(args)
        .UseSerilog()
        .ConfigureDiscordHost((context, cfg) =>
        {
            cfg.Token = config["Discord:BotToken"];
            cfg.SocketConfig = new()
            {
                UseInteractionSnowflakeDate = false,
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100
            };
        })
        .UseInteractionService((context, cfg) =>
        {
            cfg.LogLevel = LogSeverity.Info;
            cfg.UseCompiledLambda = true;
        })
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<InteractionHandler>();
            services.AddDbContext<ApplicationDbContext>(opts =>
            {
                opts.UseNpgsql(config.GetConnectionString("DefaultConnection"));
            });
        })
        .Build();

    await host.RunAsync();
}
catch (Exception e)
{
    Log.Fatal("Host terminated unexpectedly: {src} {ex} {msg}", e.Source, e.GetBaseException(), e.Message);
    throw;
}
    


static void SetupLogging(string connectionString)
    {
        IDictionary<string, ColumnWriterBase> colwriters = new Dictionary<string, ColumnWriterBase>()
        {
            { "message", new RenderedMessageColumnWriter() },
            { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
            { "raise_date", new TimestampColumnWriter() },
            { "exception", new ExceptionColumnWriter() }
        };

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.PostgreSQL(connectionString, "logs", colwriters, needAutoCreateTable:true, needAutoCreateSchema:true)
            .CreateLogger();
    }
    
static async Task SerilogConverter(LogMessage msg)
    {
        var severity = msg.Severity switch
        {
            LogSeverity.Critical => LogEventLevel.Fatal,
            LogSeverity.Error => LogEventLevel.Error,
            LogSeverity.Warning => LogEventLevel.Warning,
            LogSeverity.Info => LogEventLevel.Information,
            LogSeverity.Verbose => LogEventLevel.Verbose,
            LogSeverity.Debug => LogEventLevel.Debug,
            _ => LogEventLevel.Information
        };
        
        Log.Write(severity, msg.Exception, "[{Source}] {Message}", msg.Source, msg.Message);
        await Task.CompletedTask;
    }
