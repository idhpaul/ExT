
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using ExT.Config;
using ExT.Core.Handlers;
using ExT.Core.Modules;
using ExT.Core.Enums;
using ExT.Data;

public class Program
{
    private static IServiceProvider _services = default!;
    private static ProgramMode _mode = ProgramMode.Live;

    public static async Task Main(string[] args)
    {
        // Setup your DI container.
        _services = ConfigureServices();

        var _client = _services.GetRequiredService<DiscordSocketClient>();
        var _loggingService = _services.GetRequiredService<LoggingService>();
        var _secretConfig = _services.GetRequiredService<IConfigurationRoot>();

        await _services.GetRequiredService<InteractionHandler>().InitializeAsync();
        _services.GetRequiredService<MessageHandler>().Initialize();
        _services.GetRequiredService<ButtonExecuteHandler>().Initialize();
        _services.GetRequiredService<SqliteConnector>().Initialize();

        // Login and connect.
        await _client.LoginAsync(TokenType.Bot,
            _mode == ProgramMode.Dev ? _secretConfig["BOT_TOKEN_DEV"] : _secretConfig["BOT_TOKEN"]);
        await _client.StartAsync();

        // Wait infinitely so your bot actually stays connected.
        await Task.Delay(Timeout.Infinite);
    }

    private static IServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection()
            .AddSingleton<IConfigurationRoot>(provider => {
                return new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .Build();
            })
            .AddSingleton(provider => {
                return new BotConfig(_mode);
            })
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
            .AddSingleton<ChallengeCreateModule>()
            .AddSingleton<ChallengeUpdateModule>()
            .AddSingleton<ChallengeDeleteModule>()
            .AddSingleton<SqliteConnector>()
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
