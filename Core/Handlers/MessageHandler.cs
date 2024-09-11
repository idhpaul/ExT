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

namespace ExT.Core.Handlers
{
    public class MessageHandler
    {
        private readonly ulong _categoryId = 1282607968650793010;

        private readonly DiscordSocketClient _client;

        public MessageHandler(DiscordSocketClient client)
        {
            Console.WriteLine("MessageHandler constructor called");

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
            if (channel == null || channel.CategoryId != _categoryId) return;
            
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
                                await HandleImageUpload(attachment, message);
                            }
                        }
                    } else
                    {
                        Console.WriteLine($"{message.Author.Username}: {message.Content}");
                        await HandleTextMessage(message);
                    }
                    break;
                default:
                    // 기타 메시지 유형 처리
                    break;
            }
        }

        private async Task HandleTextMessage(IUserMessage message)
        {
            var user = message.Author;

            // 봇 메시지 작성
            var embed = new EmbedBuilder()
                .WithTitle("유의사항")
                .WithDescription("해당 채널은 \"*사진*\"만 업로드 가능합니다.")
                .WithColor(Color.Red)
                .Build();

            await message.Channel.SendMessageAsync(embed: embed);

            // 원래의 사용자 메시지는 삭제할 수도 있습니다
            await message.DeleteAsync();
        }

        private async Task HandleImageUpload(IAttachment attachment, IUserMessage message)
        {
            // 이미지 업로드 사용자 정보
            var user = message.Author;

            // 봇 메시지 작성
            var embed = new EmbedBuilder()
                .WithTitle("새로운 이미지 업로드")
                .WithDescription($"{message.Author.Mention}님이 이미지를 업로드했습니다!")
                .WithImageUrl(attachment.Url)
                .WithColor(Color.Blue)
                .Build();

            await message.Channel.SendMessageAsync(embed: embed);

            // 원래의 사용자 메시지는 삭제할 수도 있습니다
            await message.DeleteAsync();
        }

        
    }
}
