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

namespace ExT.Core.Handlers
{
    public class MessageHandler
    {
        private readonly BotConfig _config;
        private readonly DiscordSocketClient _client;

        public MessageHandler(BotConfig config, DiscordSocketClient client)
        {
            Console.WriteLine("MessageHandler constructor called");

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

        private async Task HandleImageUpload(IAttachment attachment, IUserMessage message)
        {

            using (var httpClient = new HttpClient())
            {
                // 이미지 다운로드
                var imageStream = await httpClient.GetStreamAsync(attachment.Url);

                // 파일을 메모리 스트림으로 읽기
                using (var memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0; // 스트림의 위치를 처음으로 되돌림

                    // 이미지 업로드 사용자 정보
                    var user = message.Author;

                    // 봇 메시지 작성
                    var embed = new EmbedBuilder()
                        .WithTitle("새로운 이미지 업로드")
                        .WithDescription($"{message.Author.Mention}님이 이미지를 업로드했습니다!")
                        .WithColor(Color.Blue)
                        .Build();

                    // 이미지 파일을 Discord에 재업로드
                    await message.Channel.SendFileAsync(memoryStream, "image.png");
                    await message.Channel.SendMessageAsync(embed: embed);
                }

                // 원본 메시지 삭제
                await message.DeleteAsync();
            }
        }

        
    }
}
