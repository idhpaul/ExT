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

            //
            //var buttons = new ComponentBuilder()
            //                .WithButton("업로드", "bt_imageUpload_confirm", ButtonStyle.Primary)
            //                .WithButton("취소", "bt_imageUpload_cancel", ButtonStyle.Secondary)
            //                .Build();


            Console.WriteLine("호출");
            using var httpClient = new HttpClient();
                // 이미지 다운로드
            var imageStream = await httpClient.GetStreamAsync(attachment.Url);

            // 파일을 메모리 스트림으로 읽기
            using var memoryStream = new MemoryStream();

            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // 스트림의 위치를 처음으로 되돌림

            // OpenAI Request
            ChatClient client = new(model: "gpt-4o-mini",credential:new ApiKeyCredential(_secretConfig["OPENAI_API_KEY"]));

            List<ChatMessage> messages = [
                new SystemChatMessage (
                        ChatMessageContentPart.CreateTextMessageContentPart("이미지에서 운동 데이터나 지표를 추출이 가능하면 추출하고 아니면  \"지원하지 않는 이미지 형식입니다.\" 라고 출력해." +
                                                                            "또한 이미지에서 운동 날짜 추출이 가능하면 추출하고 아니면 현재 날짜로 출력해.")
                    ),
                new UserChatMessage(
                        ChatMessageContentPart.CreateTextMessageContentPart("운동 데이터만 추출해줘. 다른 표현 및 문장은 아예 하지마."),
                        ChatMessageContentPart.CreateImageMessageContentPart(imageBytes: new BinaryData(memoryStream.ToArray()), "image/png")
                    )
            ];

            // OpenAI Response
            ChatCompletion completion = await client.CompleteChatAsync(messages);

            // 이미지 업로드 사용자 정보
            var user = message.Author;
            RestThreadChannel? existingThread = default;

            // 해당 채널의 활성화된 쓰레드 가져오기
            do
            {
                var activeThreads = await channel.GetActiveThreadsAsync();
                existingThread = activeThreads.FirstOrDefault(t => t.Name == message.Author.GlobalName);

                if (existingThread == null)
                {
                    // 사용자 이름으로 쓰레드 생성(쓰레드 삭제 불가능)
                    await channel.CreateThreadAsync(message.Author.GlobalName);
                }
            }
            while (existingThread == null);

            var fileMessage = await existingThread!.SendFileAsync(memoryStream, "image.png");
            await existingThread.SendMessageAsync($"{message.Author.Mention}님이 업로드하신 운동 기록입니다.");

            // 업로드된 파일 URL 가져오기
            var attachmentUrl = fileMessage.Attachments.FirstOrDefault()?.Url;
            if (attachmentUrl == null)
            {
                Console.WriteLine("첨부 파일 URL을 가져올 수 없습니다.");
                return;
            }

            // 봇 메시지 작성
            var embed = new EmbedBuilder()
                .WithTitle("새로운 운동 업로드")
                .WithDescription($"`{message.Author.GlobalName}`님이 운동 기록을 업로드했습니다!\n" +
                                                    $"업로드 사진 보러가기 : <#{existingThread.Id}>\n" +
                                                    $"[요약]\n{completion}")
                .WithImageUrl(attachmentUrl)
                .WithColor(Color.Blue)
                .Build();

            await message.Channel.SendMessageAsync("@everyone", embed: embed, allowedMentions: AllowedMentions.All);

            // 원본 메시지 삭제
            await message.DeleteAsync();
            
        }


    }
}
