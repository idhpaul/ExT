using Discord;
using Discord.WebSocket;
using EnumsNET;
using ExT.Config;
using ExT.Core.Enums;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Core.Handlers
{
    public class ButtonExecuteHandler
    {
        private readonly BotConfig _config;
        private readonly IConfigurationRoot _secretConfig;
        private readonly DiscordSocketClient _client;

        public ButtonExecuteHandler(BotConfig config, IConfigurationRoot secretConfig, DiscordSocketClient client)
        {
            Console.WriteLine("ButtonExecuteHandler constructor called");

            _config = config;
            _secretConfig = secretConfig;
            _client = client;
        }

        public void Initialize()
        {
            _client.ButtonExecuted += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessageComponent component)
        {
            if (component.Data.CustomId.StartsWith("bt_join"))
            {
                var parts = component.Data.CustomId.Split('_'); // "join_button_{channelId}" 형식이므로 split
                var channelId = ulong.Parse(parts[2]); // channelId 추출

                var user = component.User as IGuildUser;
                var channel = _client.GetChannel(channelId) as SocketTextChannel;
                if (user is null || channel is null)
                {
                    await component.RespondAsync("유저 or 채널을 찾을 수 없습니다.", ephemeral: true);
                }
                else
                {
                    // 사용자에게 채널 접근 권한 추가
                    var permissions = new OverwritePermissions(viewChannel: PermValue.Allow, sendMessages: PermValue.Allow);

                    // 채널에 해당 사용자의 권한 부여
                    await channel.AddPermissionOverwriteAsync(user, permissions);

                    var embed = new EmbedBuilder()
                                .WithTitle("채널 이동하기")
                                .WithDescription($"👉 {channel.Mention}\n" +
                                $"> * 채널 이동 후 운동 기록 사진을 업로드하세요.\n")
                                .WithColor(Color.Blue)
                                .Build();

                    await component.RespondAsync($"{user.Username}님이 비공개 채널에 접근할 수 있도록 설정되었습니다.", embed: embed, ephemeral: true);
                }

            } 
            //else if (component.Data.CustomId.Equals("bt_imageUpload_confirm"))
            //{

            //}
            //else if (component.Data.CustomId.Equals("bt_imageUpload_cancel"))
            //{

            //}

            //// We can now check for our custom id
            //switch (component.Data.CustomId)
            //{
            //    // Since we set our buttons custom id as 'custom-id', we can check for it like this:
            //    case "custom-id":
            //        // Lets respond by sending a message saying they clicked the button
            //        await component.RespondAsync($"{component.User.Mention} has clicked the button!");
            //        break;
            //}
        }
    }
}
