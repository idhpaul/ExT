using Discord.Interactions;
using Discord;
using ExT.Config;
using ExT.Core.Attribute;
using ExT.Core.Enums;
using ExT.Data;
using System.Text.RegularExpressions;

namespace ExT.Core.Modules
{
    public class ChallengeUpdateModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BotConfig _config;
        private SqliteConnector _sqlite;

        public ChallengeUpdateModule(BotConfig config, SqliteConnector sqlite)
        {
            Console.WriteLine("ChallengeUpdateModule constructor called");

            _config = config;
            _sqlite = sqlite;
        }

        [SlashCommand("도전수정", "[리더 전용] 도전 임베드 내용을 수정합니다.")]
        [RequireCommandRole(Role.Leader)]
        public async Task ChallengeUpdate(
            [Summary("메시지ID", "수정할 도전 임베드 메시지의 ID입니다.")] string messageId)
        {

            var beforeMessage = await MessageUtil.GetMessageFromChannel(Context.Channel, Convert.ToUInt64(messageId));
            if (beforeMessage is null)
            {
                await RespondAsync("수정할 메시지를 찾을 수 없습니다.",ephemeral:true);
                return;
            }

            // 정규식으로 숫자만 추출
            var extractUserId = Regex.Match(beforeMessage.Embeds.First().Description, @"<@(\d+)>");
            if (!extractUserId.Success)
            {
                await RespondAsync("리더 정보를 찾을 수 없습니다.", ephemeral: true);
                return;
            }

            if (Context.User.Id.ToString() != extractUserId.Groups[1].Value)
            {
                await RespondAsync("해당 도전을 등록한 리더만 수정할 수 있습니다.", ephemeral: true);
                return;
            }

            // 모달 수정 및 상호작용 핸들러 연결
            await Context.Interaction.RespondWithModalAsync<ChallengeUpdateModalContext>($"md_id_updateChallenge:{messageId}", modifyModal: builder =>
            {
                builder
                    .WithTitle("📌 도전 수정")
                    .UpdateTextInput(
                        "md_lb_updateChallenge_channelname",
                        updateTextInput: builder =>
                        {
                            builder.WithValue(beforeMessage.Embeds.FirstOrDefault()?.Title ?? "");
                        });

            });

        }

        public class ChallengeUpdateModalContext : IModal
        {
            public string Title => "📌 도전 수정";

            [InputLabel("채널 이름 앞 `도전` 이 붙습니다. (띄어쓰기 - 기호 대체)")]
            [RequiredInput(true)]
            [ModalTextInput("md_lb_updateChallenge_channelname", placeholder: "채널명을 입력해주세요", maxLength: 45)]
            public required string ChannelName { get; set; }

        }

        [ModalInteraction("md_id_updateChallenge:(\\d+)", TreatAsRegex = true)]
        public async Task ModalResponse(string messageId, ChallengeUpdateModalContext modal)
        {
            var beforeMessage = await MessageUtil.GetMessageFromChannel(Context.Channel, Convert.ToUInt64(messageId));
            if (beforeMessage is null)
            {
                await RespondAsync("해당 메시지를 찾을 수 없습니다.", ephemeral:true);
                return;
            }

            var embed = new EmbedBuilder()
                            .WithTitle(modal.ChannelName)
                            .WithDescription($"리더 : {Context.User.Mention}")
                            .WithColor(Color.Blue) // 색상 설정
                            .WithThumbnailUrl("https://cdn.discordapp.com/attachments/1290685382651809813/1290685388272439296/challengeThumbnail.jpg?ex=66fd5bf0&is=66fc0a70&hm=36d096f93b555631e7184a1b7531e9fa65babb0f7a6559b4ef66f58d56ada8c5&")
                            .Build();

            // 임베드 메시지 수정
            await beforeMessage.ModifyAsync(msg =>
            {
                msg.Embeds = new[] { embed };
            });

            await RespondAsync("메시지가 성공적으로 수정되었습니다.", ephemeral: true);
        }

    }    
}
