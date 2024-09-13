
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Net;
using Newtonsoft.Json;
using ExT.Core;
using Discord.Interactions;
using System.Globalization;
using ExT.Core.Handlers;
using ExT.Core.config;
using ExT.Core.Modules;


public class Program
{
    private static IServiceProvider _services = default!;

    public static async Task Main(string[] args)
    {
        // Setup your DI container.
        _services = ConfigureServices();

        var _client = _services.GetRequiredService<DiscordSocketClient>();
        var _loggingService = _services.GetRequiredService<LoggingService>();

        // Secrets in non-web applications (secrets.json)
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
        _services.GetRequiredService<MessageHandler>().Initialize();
        _services.GetRequiredService<ButtonExecuteHandler>().Initialize();

        // Login and connect.
        await _client.LoginAsync(TokenType.Bot, config["BOT_TOKEN"]);
        await _client.StartAsync();

        // Wait infinitely so your bot actually stays connected.
        await Task.Delay(Timeout.Infinite);
    }

    private static IServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection()
            .AddSingleton<BotConfig>()
            .AddSingleton(provider => {
                var config = new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 100,
                    GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers | GatewayIntents.MessageContent,
                    AlwaysDownloadUsers = true,
                };

                return new DiscordSocketClient(config);
            })
            .AddSingleton(provider => new InteractionService(provider.GetRequiredService<DiscordSocketClient>().Rest, _interactionServiceConfig))
            .AddSingleton<InteractionHandler>()
            .AddSingleton<MessageHandler>()
            .AddSingleton<ButtonExecuteHandler>()
            .AddSingleton<RegistExerciseModalModule>()
            .AddSingleton<LoggingService>();

        return serviceCollection.BuildServiceProvider();
    }

    private static readonly InteractionServiceConfig _interactionServiceConfig = new InteractionServiceConfig
    {
        LogLevel = LogSeverity.Verbose,
        //LocalizationManager = new ResxLocalizationManager("InteractionFramework.Resources.CommandLocales", Assembly.GetEntryAssembly(),
        //    new CultureInfo("ko-KR"), new CultureInfo("en-US"))
    };

}
