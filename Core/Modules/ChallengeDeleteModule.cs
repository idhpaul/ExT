using Discord.Interactions;
using Discord;
using ExT.Config;
using ExT.Core.Attribute;
using ExT.Core.Enums;
using ExT.Data;
using System.Text.RegularExpressions;
using ExT.Data.Entities;

namespace ExT.Core.Modules
{
    public class ChallengeDeleteModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BotConfig _config;
        private SqliteConnector _sqlite;

        public ChallengeDeleteModule(BotConfig config, SqliteConnector sqlite)
        {
            Console.WriteLine("ChallengeDeleteModule constructor called");

            _config = config;
            _sqlite = sqlite;
        }

        [SlashCommand("도전삭제", "[리더 전용] 도전 임베드 내용을 삭제합니다.")]
        [RequireCommandRole(Role.Leader)]
        public async Task ChallengeUpdate(
            [Summary("메시지ID", "삭제할 도전 임베드 메시지의 ID입니다.")] string messageId)
        {

            var beforeMessage = await MessageUtil.GetMessageFromChannel(Context.Channel, Convert.ToUInt64(messageId));
            if (beforeMessage is null)
            {
                await RespondAsync("삭제할 메시지를 찾을 수 없습니다.",ephemeral:true);
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
                await RespondAsync("해당 도전을 등록한 리더만 삭제할 수 있습니다.", ephemeral: true);
                return;
            }

            // 모달 수정 및 상호작용 핸들러 연결
            await Context.Interaction.RespondWithModalAsync<ChallengeUpdateModalContext>($"md_id_deleteChallenge:{messageId}");

        }

        public class ChallengeUpdateModalContext : IModal
        {
            public string Title => "📌 도전 삭제 확인";

            [InputLabel("`삭제` 를 입력하시면 도전이 삭제 됩니다.")]
            [RequiredInput(true)]
            [ModalTextInput("md_lb_deleteChallenge_channelname", maxLength: 2)]
            public required string ConfirmText { get; set; }

        }

        [ModalInteraction("md_id_deleteChallenge:(\\d+)", TreatAsRegex = true)]
        public async Task ModalResponse(string messageId, ChallengeUpdateModalContext modal)
        {

            if (modal.ConfirmText != "삭제")
            {
                await RespondAsync("`삭제` 확인 메시지를 정확히 입력해주세요.", ephemeral: true);
                return;
            }

            var beforeMessage = await MessageUtil.GetMessageFromChannel(Context.Channel, Convert.ToUInt64(messageId));
            if (beforeMessage is null)
            {
                await RespondAsync("해당 메시지를 찾을 수 없습니다.", ephemeral:true);
                return;
            }

            try
            {
                // DB 도전 조회
                ChallengeEntity challenge = await _sqlite.DbSelectChallenge(messageId: beforeMessage.Id);

                // 채널 삭제
                await ChannelUtil.DeleteChannelFromChannelId(Context, challenge.ChannelId);

                // 메시지 삭제
                await beforeMessage.DeleteAsync();

                // DB 도전 목록 삭제 commit
                _sqlite.DbDeleteChallenge(challenge);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"at md_id_deleteChallenge:(\\d+): {ex.Message}");
                throw;
            }

            await RespondAsync("성공적으로 삭제되었습니다.", ephemeral: true);

        }

    }    
}
