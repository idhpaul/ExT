using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using ExT.Core.config;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExT.Core.Modules
{
    public class UploadConfirmModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BotConfig _config;
        private readonly IConfigurationRoot _secretConfig;
        private readonly DiscordSocketClient _client;

        public UploadConfirmModule(BotConfig config, IConfigurationRoot secretConfig, DiscordSocketClient client)
        {
            Console.WriteLine("UploadConfirmModule constructor called");

            _config = config;
            _secretConfig = secretConfig;
            _client = client;
        }
        [ComponentInteraction("bt_imageUpload_confirm:*,*")]
        public async Task ButtonImageUploadConfirm(string channelId, string messageId)
        {
            // 채널을 가져옵니다.
            var channel = _client.GetChannel(Convert.ToUInt64(channelId)) as SocketTextChannel;
            if (channel is null)
            {
                return;
            }

            // 메시지를 가져옵니다.
            var message = await channel.GetMessageAsync(Convert.ToUInt64(messageId)) as IMessage;
            if (message is null)
            {
                await RespondAsync("원본 메시지를 찾을 수 없습니다.", ephemeral: true);
                return;
            }

            var userId = Context.User.Id;
            if (message.Author.Id != userId)
            {
                await RespondAsync("메시지 올린 본인만 업로드 요청이 가능합니다.", ephemeral: true);
            }

            var interaction = Context.Interaction as SocketMessageComponent;
            if (interaction is not null)
            {
                // 상호작용했던 메시지 삭제
                await interaction.Message.DeleteAsync();
            }

            // 이미지 파일 확인
            var imageAttachments = message.Attachments
                                    .Where(a => a.ContentType.StartsWith("image/") && a.ContentType != "image/gif" && a.ContentType != "image/webp")
                                    .ToList();
            if (imageAttachments.Count is 0)
            {
                await RespondAsync("지원하지 않는 형식입니다.", ephemeral: true);
                return;
            }

            await RespondAsync("분석 중입니다.", ephemeral: true);

            foreach (var image in imageAttachments)
            {
                using var httpClient = new HttpClient();
                var imageStream = await httpClient.GetStreamAsync(image.Url);

                using var memoryStream = new MemoryStream();

                await imageStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0; // 스트림의 위치를 처음으로 되돌림

                // OpenAI Request
                ChatClient client = new(model: "gpt-4o-mini", credential: new ApiKeyCredential(_secretConfig["OPENAI_API_KEY"]!));

                List<ChatMessage> gptMessages = [
                    new SystemChatMessage (
                        ChatMessageContentPart.CreateTextMessageContentPart("이미지에서 운동 데이터나 지표를 추출이 가능하면 추출하고 아니면  \"지원하지 않는 이미지 형식입니다.\" 라고 출력해." +
                                                                            "또한 운동 시간과 칼로리 소비량을 반드시 포함하고, 운동 시간 혹은 칼로리 소비량이 없으면 '데이터 없음'이라고 표시해." +
                                                                            "또한 운동 시간과 칼로리 소비량과 관련 없는 것들은 따로 한번에 분류하고 없으면 '데이터 없음' 이라고 표시해.")
                    ),
                    new UserChatMessage(
                            ChatMessageContentPart.CreateTextMessageContentPart("운동 데이터만 추출해."),
                            ChatMessageContentPart.CreateImageMessageContentPart(imageBytes: new BinaryData(memoryStream.ToArray()), "image/png")
                    )
                ];

                ChatCompletionOptions options = new()
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                                        name: "exercise_data",
                                        jsonSchema: BinaryData.FromString("""
                                                {
                                                    "type": "object",
                                                    "properties": {
                                                        "exercise_time": { 
                                                            "type": "string",
                                                            "description": "운동 시간"
                                                        },
                                                        "calories_burned": { 
                                                            "type": "string",
                                                            "description": "소모 칼로리"
                                                        },
                                                        "other_data": {
                                                            "type": "string",
                                                            "description": "기타 운동 관련 데이터"
                                                        }
                                                    },
                                                    "required": ["exercise_time", "calories_burned", "other_data"],
                                                    "additionalProperties": false
                                                }
                                            """),
                                        strictSchemaEnabled: true)
                };

                // OpenAI Response
                ChatCompletion chatCompletion = await client.CompleteChatAsync(gptMessages,options);

                Console.WriteLine($"input token : {chatCompletion.Usage.InputTokens}\n" +
                                    $"output token : {chatCompletion.Usage.OutputTokens}\n" +
                                    $"[Total token] : {chatCompletion.Usage.TotalTokens}");

                using JsonDocument structuredJson = JsonDocument.Parse(chatCompletion.ToString());

                Console.WriteLine($"Exercise Time: {structuredJson.RootElement.GetProperty("exercise_time").GetString()}");
                Console.WriteLine($"Calories Burned: {structuredJson.RootElement.GetProperty("calories_burned").GetString()}");

                // 이미지 업로드 사용자 정보
                var user = message.Author;
                RestThreadChannel? existingThread = default;

                // 해당 채널의 활성화된 쓰레드 가져오기
                do
                {
                    var activeThreads = await channel.GetActiveThreadsAsync();
                    existingThread = activeThreads.FirstOrDefault(t => t.Name == message.Author.GlobalName);

                    if (existingThread is null)
                    {
                        // 사용자 이름으로 쓰레드 생성(쓰레드 삭제 불가능)
                        await channel.CreateThreadAsync(message.Author.GlobalName);
                    }
                }
                while (existingThread is null);

                var fileMessage = await existingThread!.SendFileAsync(memoryStream, "image.png");
                await existingThread.SendMessageAsync($"{message.Author.Mention}님이 업로드하신 운동 기록입니다.");

                // 업로드된 파일 URL 가져오기
                var attachmentUrl = fileMessage.Attachments.FirstOrDefault()?.Url;
                if (attachmentUrl is null)
                {
                    Console.WriteLine("첨부 파일 URL을 가져올 수 없습니다.");
                    return;
                }

                // 봇 메시지 작성
                var embedData = new EmbedBuilder()
                    .WithTitle("💪 새로운 운동 기록")
                    .AddField(name: "🔥 Data", value: $"{chatCompletion}")
                    .WithFooter($"- from {message.Author.GlobalName}")
                    .WithColor(Color.Gold)
                    .Build();

                var embedImage = new EmbedBuilder()
                    .WithTitle("🖼️ Image")
                    .WithDescription($"업로드 사진 보러가기 : <#{existingThread.Id}>")
                    .WithImageUrl(attachmentUrl)
                    .WithColor(Color.Orange)
                    .Build();

                await message.Channel.SendMessageAsync("@everyone", embeds: [embedData, embedImage], allowedMentions: AllowedMentions.All);

            }

            // 분석 중 확인 메시지(ephemeral) 삭제
            await DeleteOriginalResponseAsync();

            await FollowupAsync("`분석 완료`되었습니다.", ephemeral: true);

            // 사용자가 업로드한 사진 메시지 삭제
            await message.DeleteAsync();

        }

        [ComponentInteraction("bt_imageUpload_cancel:*,*")]
        public async Task ButtonImageUploadCancel(string channelId, string messageId)
        {
            await MessageUtil.FindDeleteMessage(_client, channelId, messageId);
        }
    }
}
