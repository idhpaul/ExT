using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using ExT.Core.config;
using OpenAI.Chat;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using Discord.Rest;
using System.Threading;

namespace ExT.Core.Handlers
{
    public class MessageHandler
    {
        private readonly IConfigurationRoot _secretConfig;
        private readonly BotConfig _config;
        private readonly DiscordSocketClient _client;

        public MessageHandler(IConfigurationRoot secretConfig, BotConfig config, DiscordSocketClient client)
        {
            Console.WriteLine("MessageHandler constructor called");

            _secretConfig = secretConfig;
            _config = config;
            _client = client;
        }

        public void Initialize()
        {
            _client.MessageReceived += OnMessageReceivedAsync;
        }


        private async Task OnMessageReceivedAsync(SocketMessage arg)
        {
            // 봇이 보낸 메시지인 경우 무시
            if (arg.Author.IsBot) return;
            if (arg is not IUserMessage message) return;

            // 채널이 지정된 카테고리에 속하는지 확인
            var channel = message.Channel as SocketTextChannel;

            if (channel == null || channel.CategoryId != _config.privateCategoryID) return;
            if (channel.GetChannelType() == ChannelType.PublicThread) return;

            switch (message.Type)
            {
                case MessageType.Default:
                case MessageType.Reply:
                    // 일반 메시지 및 답글 메시지 처리
                    if (message.Attachments.Any())
                    {
                        foreach (var attachment in message.Attachments)
                        {
                            if (attachment.Width.HasValue && attachment.Height.HasValue)
                            {
                                // 이미지 파일로 판단
                                await HandleImageUpload(attachment, message, channel);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{message.Author.Username}: {message.Content}");
                        //await HandleTextMessage(message);
                    }
                    break;
                default:
                    // 기타 메시지 유형 처리
                    break;
            }
        }

        private async Task HandleTextMessage(IUserMessage message)
        {
            // 사용자 메시지 삭제
            await message.DeleteAsync();

            var user = message.Author;

            // 봇 메시지 작성      
            var embed = new EmbedBuilder()
                .WithTitle("유의사항")
                .WithDescription("해당 채널은 \"*사진*\"만 업로드 가능합니다.")
                .WithColor(Color.Red)
                .Build();

            var botMessage = await message.Channel.SendMessageAsync(embed: embed);
            await Task.Delay(5000);
            await botMessage.DeleteAsync();
        }

        private async Task HandleImageUpload(IAttachment attachment, IUserMessage message, SocketTextChannel channel)
        {
            var buttons = new ComponentBuilder()
                            .WithButton("분석 시작", $"bt_imageUpload_confirm:{channel.Id},{message.Id}", ButtonStyle.Primary)
                            .WithButton("취소", "bt_imageUpload_cancel", ButtonStyle.Secondary)
                            .Build();

            var uploadConfirmMessage = await message.ReplyAsync($"`{message.Author.GlobalName}님.`\n" +
                                    $"## 위 이미지를 `운동 데이터 분석` 할까요? \n" +
                                    $"> * 분석 요청은 사진을 업로드한 사람만 가능합니다.\n" +
                                    $"> * 이 메시지는 1분 후 자동 삭제됩니다.", components: buttons);
        }


    }
}
