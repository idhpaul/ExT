using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ExT.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExT.Core.Attribute;
using ExT.Core.Enums;
using EnumsNET;
using ExT.Config;
using ExT.Data;
using ExT.Data.Entities;

namespace ExT.Core.Modules
{
    public class ChallengeCreateModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BotConfig _config;
        private InteractionHandler _handler;
        private SqliteConnector _sqlite;

        public ChallengeCreateModule(BotConfig config, InteractionHandler handler, SqliteConnector sqlite)
        {
            Console.WriteLine("ChallengeCreateModule constructor called");

            _config = config;
            _handler = handler;
            _sqlite = sqlite;
    }

        [SlashCommand("도전등록", "[리더 전용] 도전 임베드 메시지 및 해당 채널을 생성합니다.")]
        [RequireCommandRole(Role.Leader)]
        public async Task RegistChallenge()
        {
            await Context.Interaction.RespondWithModalAsync<ChallengeCreateModalContext>("md_id_createChallenge");
        }

        public class ChallengeCreateModalContext : IModal
        {
            public string Title => "📌 도전 등록";

            [InputLabel("채널 이름 앞 `도전` 이 붙습니다.(수정 불가) (띄어쓰기 - 기호 대체)")]
            [RequiredInput(true)]
            [ModalTextInput("md_lb_regChallenge_channelname", placeholder: "채널명을 입력해주세요", maxLength: 45)]
            public required string ChannelName { get; set; }

        }

        [ModalInteraction("md_id_createChallenge")]
        public async Task ModalResponse(ChallengeCreateModalContext modal)
        {
            // 채널 중복 확인

            // 채널 생성
            var guild = Context.Guild;
            var developerRole = guild.Roles.FirstOrDefault(r => r.Name == Role.Developer.AsString(EnumFormat.Description));
            if (developerRole is null)
            {
                await RespondAsync("`Developer🚀` 역할을 찾을 수 없습니다. 설정을 확인해주세요.", ephemeral: true);
                return;
            }

            var developerRoleId = developerRole.Id;

            // 도전 채널 생성
            var privateChannel = await guild.CreateTextChannelAsync($"도전 {modal.ChannelName}", properties =>
            {
                properties.CategoryId = _config.privateCategoryID; // 카테고리 ID
                properties.Topic = $"{modal.ChannelName} 채널입니다.";
                properties.PermissionOverwrites = new[]
                {
                    new Overwrite(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)), // 모든 사용자에게 비공개
                    new Overwrite(Context.Client.CurrentUser.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)), // 봇 권한
                    new Overwrite(developerRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Allow)) // 개발자 역할 권한
                };
            });

            // 도전 채널 안내 메시지
            await privateChannel.SendMessageAsync($"# 💪 채널 이용 방법\r\n" +
                $"> 🔸 이 채널은 도전에 참가한 사람들에게만 보여집니다.\r\n" +
                $"> 🔸 채널에 사진을 업로드하면 `분석` 후 `요약 결과` 를 볼 수 있습니다.\r\n" +
                $"> 🔸 각자 업로드한 사진은 쓰레드에 보관됩니다.\r\n" +
                $"> 🔸 도전 참가자들 간 자유로운 대화 가능합니다.\r\n" +
                $"> 🔸 상호 간 존중 및 예의를 지켜주세요.");

            var embed = new EmbedBuilder()
                            .WithTitle(modal.ChannelName)
                            .WithDescription($"리더 : {Context.User.Mention}")
                            .WithThumbnailUrl("https://cdn.discordapp.com/attachments/1290685382651809813/1290685388272439296/challengeThumbnail.jpg?ex=66fd5bf0&is=66fc0a70&hm=36d096f93b555631e7184a1b7531e9fa65babb0f7a6559b4ef66f58d56ada8c5&")
                            .WithColor(Color.Blue)
                            .WithTimestamp(DateTimeOffset.Now) // 글로벌 환경이 아니기에 현지시간(KST) 함수 사용
                            .Build();

            var buttons = new ComponentBuilder()
                            .WithButton("Join", $"bt_join_{privateChannel.Id}", ButtonStyle.Primary)
                            //.WithButton("Detail", "bt_detail", ButtonStyle.Secondary)
                            .Build();

           await RespondAsync(embed: embed, components: buttons);

            // 전송된 메시지 가져오기
            var sentMessage = await GetOriginalResponseAsync();
            var messageId = sentMessage.Id;

            try
            {
                // DB 도전 목록 commit
                _sqlite.DbInsertChallenge(
                    new ChallengeEntity
                    {
                        Title = modal.ChannelName,
                        MessageId = messageId,
                        ChannelId = privateChannel.Id,
                        LeaderName = Context.User.GlobalName,
                        LeaderId = Context.User.Id
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"at md_id_createChallenge : {ex.Message}");
                throw;
            }
            
        }
    }
}
