using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class MessageUtil
{
    public static async Task DelayDeleteMessage(TimeSpan span, IUserMessage targetMessage)
    {
        await Task.Delay(delay: span);

        if (targetMessage is not null)
            await targetMessage.DeleteAsync();
    }

    public static async Task FindDeleteMessage(DiscordSocketClient client, ulong channelId, ulong messageId)
    {
        SocketTextChannel? channel = client.GetChannel(channelId) as SocketTextChannel;
        if (channel is null)
        {
            Console.WriteLine("failed to get channel");
            return;
        }
        else
        {
            var message = await channel.GetMessageAsync(messageId);
            if (message is not null)
                await message.DeleteAsync();
        }
    }

    public static async Task FindDeleteMessage(DiscordSocketClient client, string channelId, string messageId)
    {
        Debug.Assert(channelId is not null, "channelId parameter is null");
        Debug.Assert(messageId is not null, "messageId parameter is null");

        SocketTextChannel? channel = client.GetChannel(Convert.ToUInt64(channelId)) as SocketTextChannel;
        if (channel is null)
        {
            Console.WriteLine("failed to get channel");
            return;
        }
        else
        {
            var message = await channel.GetMessageAsync(Convert.ToUInt64(messageId));
            if (message is not null)
                await message.DeleteAsync();
        }  
    }
}
