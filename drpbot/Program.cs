using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile("appsettings.Development.json", optional:true)
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
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 100
            };
        })
        // .UseCommandService((context, cfg) =>
        // {
        //     cfg.DefaultRunMode = RunMode.Async;
        //     cfg.CaseSensitiveCommands = false;
        // })
        .UseInteractionService((context, cfg) =>
        {
            cfg.LogLevel = LogSeverity.Info;
            cfg.UseCompiledLambda = true;
        })
        .ConfigureServices((context, services) =>
        {
        
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
            .WriteTo.PostgreSQL(connectionString, "logs", colwriters)
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
