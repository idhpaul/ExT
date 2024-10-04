using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using ExT.Config;
using ExT.Data;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using Dapper;
using System.Data.SQLite;
using System.ClientModel;
using System.Text.Json;
using ExT.Data.Entities;
using System.Diagnostics;
using ExT.Service;
using ExT.Core.Attribute;
using ExT.Core.Enums;
using System.Text.RegularExpressions;
using System.Threading.Channels;


namespace ExT.Core.Modules
{
    public class PrintGroupExerciseProgressImageModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BotConfig _config;
        private readonly IConfigurationRoot _secretConfig;
        private readonly DiscordSocketClient _client;
        private SqliteConnector _sqlite;

        public PrintGroupExerciseProgressImageModule(BotConfig config, IConfigurationRoot secretConfig, DiscordSocketClient client, SqliteConnector sqlite)
        {
            Console.WriteLine("PrintGroupExerciseProgressImageModule constructor called");

            _config = config;
            _secretConfig = secretConfig;
            _client = client;
            _sqlite = sqlite;
        }

        [SlashCommand("진행사항", "[리더 전용] ")]
        public async Task ProgressPrint(
            [Summary("채널ID", "삭제할 도전 임베드 메시지의 ID입니다.")] string channelId)
        {

            await DeferAsync();

            // 요약 사진 업데이트
            var foo = _sqlite.DbSelectExercise(Convert.ToUInt64(channelId));
            foreach (var exercise in foo)
            {
                Console.WriteLine($"ExerciseTime: {exercise.ExerciseTime}, CaloriesBurned: {exercise.CaloriesBurned}, OtherData: {exercise.OtherData}, UserName: {exercise.UserName}");
            }

            string filePath = @"test.webp";

            var image1 = new GroupExerciseProgressImage(foo!);
            image1.GenerateImage(filePath);


            if (File.Exists(filePath))
            {
                // 파일 스트림을 열고 응답으로 파일을 전송
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    // RespondWithFileAsync를 사용하여 파일 스트림을 전송
                    await Context.Channel.SendFileAsync(stream, Path.GetFileName(filePath), "현재 진행사항 입니다.");
                }
            }
            else
            {
                // 파일이 없을 경우 오류 메시지 전송
                await ReplyAsync("File not found!");
            }

        }
    }
}
