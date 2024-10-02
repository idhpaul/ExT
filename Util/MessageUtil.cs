using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

public static class MessageUtil
{
    public static async Task DelayDeleteMessage(TimeSpan span, IUserMessage targetMessage)
    {
        await Task.Delay(delay: span);

        if (targetMessage is not null)
            await targetMessage.DeleteAsync();
    }

    public static async Task<IUserMessage?> GetMessageFromChannel(ISocketMessageChannel channel, ulong messageId)
    {
        Debug.Assert(channel is not null, "channelId parameter is null");
        Debug.Assert(messageId > 0, "messageId parameter is null");

        var message = await channel.GetMessageAsync(messageId) as IUserMessage;

        return (message is null) ? null : message;
    }

    public static async Task FindDeleteMessage(DiscordSocketClient client, string channelId, string messageId)
    {
        Debug.Assert(channelId is not null, "channelId parameter is null");
        Debug.Assert(messageId is not null, "messageId parameter is null");

        SocketTextChannel? channel = client.GetChannel(Convert.ToUInt64(channelId)) as SocketTextChannel;
        if (channel is null)
        {
            throw new Exception("channel is null");
        }
        else
        {
            var message = await channel.GetMessageAsync(Convert.ToUInt64(messageId));
            if (message is not null)
                await message.DeleteAsync();
        }  
    }
}
