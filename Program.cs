
using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Discord;
using Discord.Commands;
using Discord.WebSocket;


public class Program
{
    private static DiscordSocketClient _client = default!;
    private static CommandService _commands = default!;
    private static IServiceProvider _services = default!;

    private static LoggingService _log = default!;
    public static async Task Main()
    {
        _client = new DiscordSocketClient();
        _commands = new CommandService();

        // Setup your DI container.
        _services = ConfigureServices();

        // Secrets in non-web applications (secrets.json)
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        // Centralize the logic for commands into a separate method.
        await InitCommands();

        // Login and connect.
        await _client.LoginAsync(TokenType.Bot,
            // < DO NOT HARDCODE YOUR TOKEN >
            config["BOT_TOKEN"]);
        await _client.StartAsync();

        // Wait infinitely so your bot actually stays connected.
        await Task.Delay(Timeout.Infinite);
    }

    private static IServiceProvider ConfigureServices()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<LoggingService>();

        return serviceCollection.BuildServiceProvider();
    }

    private static async Task InitCommands()
    {
        // Either search the program and add all Module classes that can be found.
        // Module classes MUST be marked 'public' or they will be ignored.
        // You also need to pass your 'IServiceProvider' instance now,
        // so make sure that's done before you get here.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        // Or add Modules manually if you prefer to be a little more explicit:
        //await _commands.AddModuleAsync<SomeModule>(_services);
        // Note that the first one is 'Modules' (plural) and the second is 'Module' (singular).

        // Subscribe a handler to see if a message invokes a command.
        _client.MessageReceived += HandleCommandAsync;
    }

    private static async Task HandleCommandAsync(SocketMessage arg)
    {
        // Bail out if it's a System Message.
        var msg = arg as SocketUserMessage;
        if (msg == null) return;

        // We don't want the bot to respond to itself or other bots.
        if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot) return;

        // Create a number to track where the prefix ends and the command begins
        int pos = 0;
        // Replace the '!' with whatever character
        // you want to prefix your commands with.
        // Uncomment the second half if you also want
        // commands to be invoked by mentioning the bot instead.
        //if (msg.HasCharPrefix('!', ref pos)  || msg.HasMentionPrefix(_client.CurrentUser, ref pos) )
        //{
        //    // Create a Command Context.
        //    var context = new SocketCommandContext(_client, msg);

        //    // Execute the command. (result does not indicate a return value, 
        //    // rather an object stating if the command executed successfully).
        //    var result = await _commands.ExecuteAsync(context, pos, _services);

        //    // Uncomment the following lines if you want the bot
        //    // to send a message if it failed.
        //    // This does not catch errors from commands with 'RunMode.Async',
        //    // subscribe a handler for '_commands.CommandExecuted' to see those.
        //    //if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
        //    //    await msg.Channel.SendMessageAsync(result.ErrorReason);
        //}

        if (msg.HasCharPrefix('!', ref pos))
        {
            // '!' 뒤의 문자열을 가져옵니다.
            var command = msg.Content.Substring(pos).Trim();

            // 명령어가 비어있지 않으면, 해당 메시지를 에코합니다.
            if (!string.IsNullOrEmpty(command))
            {
                await msg.Channel.SendMessageAsync(command);
            }
        }
    }
}
