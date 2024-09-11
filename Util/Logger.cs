using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;

public class LoggingService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _handler;

    public LoggingService(DiscordSocketClient client, InteractionService handler)
    {
        Console.WriteLine("LoggingService constructor called");

        _client = client;
        _handler = handler;

        _client.Log += LogAsync;
        _handler.Log += LogAsync;
    }
    private Task LogAsync(LogMessage message)
    {
        if (message.Exception is CommandException cmdException)
        {
            Console.WriteLine($"[Command/{message.Severity}] {cmdException.Command.Aliases.First()}"
                + $" failed to execute in {cmdException.Context.Channel}.");
            Console.WriteLine(cmdException);
        }
        else
            Console.WriteLine($"[General/{message.Severity}] {message}");

        return Task.CompletedTask;
    }
}