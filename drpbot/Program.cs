using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional:true)
            .Build();
        
        SetupLogging(config.GetConnectionString("DefaultConnection"));
        
        try
        {
            Log.Verbose("Discord RPBot starting...");
            var BotClient = new DiscordSocketClient();
            BotClient.Log += SerilogConverter;
            await BotClient.LoginAsync(TokenType.Bot, config["Discord:BotToken"]);
            await BotClient.StartAsync();

            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Program failed to start");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    
    // private static ServiceProvider ConfigureServices()
    // {
        
    // }
    
    private static void SetupLogging(string connectionString)
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
    
    private static async Task SerilogConverter(LogMessage msg)
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
}