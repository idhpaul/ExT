using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CommandUtils
{
    public static async Task GetGuildCommands(DiscordSocketClient client, ulong guildId)
    {
        var guild = client.GetGuild(guildId);
        var commands = await guild.GetApplicationCommandsAsync();

        foreach (var command in commands)
        {
            Console.WriteLine($"guild command: {command.Name}");
        }
    }

    public static async Task DeleteAllGuildCommands(DiscordSocketClient client, ulong guildId)
    {
        var guild = client.GetGuild(guildId);
        var commands = await guild.GetApplicationCommandsAsync();

        foreach (var command in commands)
        {
            await command.DeleteAsync();
            Console.WriteLine($"Deleted guild command: {command.Name}");
        }
    }

    public static async Task DeleteSpecificGuildCommand(DiscordSocketClient client, ulong guildId, string commandName)
    {
        var guild = client.GetGuild(guildId);
        var commands = await guild.GetApplicationCommandsAsync();

        foreach (var command in commands)
        {
            if (command.Name == commandName)
            {
                await command.DeleteAsync();
                Console.WriteLine($"Deleted specific guild command: {command.Name}");
            }
        }
    }

    public static async Task GetGlobalCommands(DiscordSocketClient client)
    {
        var commands = await client.GetGlobalApplicationCommandsAsync();

        foreach (var command in commands)
        {
            Console.WriteLine($"guild command: {command.Name}");
        }
    }

    public static async Task DeleteAllGlobalCommands(DiscordSocketClient client)
    {
        var commands = await client.GetGlobalApplicationCommandsAsync();

        foreach (var command in commands)
        {
            await command.DeleteAsync();
            Console.WriteLine($"Deleted global command: {command.Name}");
        }
    }
}

