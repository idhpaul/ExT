using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ChannelUtil
{
    public static SocketTextChannel? GetChannelFromChannelId(SocketInteractionContext context, ulong channelId)
    {
        Debug.Assert(context is not null, "context parameter is null");
        Debug.Assert(channelId > 0, "channelId parameter is null");

        var guild = (SocketGuild)context.Guild;

        var channel = guild.GetChannel(channelId) as SocketTextChannel; // 텍스트 채널로 변환
        return (channel is null) ? null : channel;
    }

    public static async Task DeleteChannelFromChannelId(SocketInteractionContext context, ulong channelId)
    {
        Debug.Assert(context is not null, "context parameter is null");
        Debug.Assert(channelId > 0, "channelId parameter is null");

        var guild = (SocketGuild)context.Guild;

        // exception 처리 완성하기
        var channel = guild.GetChannel(channelId) as SocketTextChannel; // 텍스트 채널로 변환
        if (channel is null)
        {
            throw new Exception("channel is null");
        }
        else
        {
            await channel.DeleteAsync();
        }
    }

}
