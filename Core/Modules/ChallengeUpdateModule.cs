using Discord.Interactions;
using Discord;
using EnumsNET;
using ExT.Config;
using ExT.Core.Attribute;
using ExT.Core.Enums;
using ExT.Core.Handlers;
using ExT.Data.Entities;
using ExT.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static ExT.Core.Modules.ChallengeUpdateModal;
using Discord.Rest;

namespace ExT.Core.Modules
{
    public class ChallengeUpdateModule : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionHandler _handler;

        public ChallengeUpdateModule(InteractionHandler handler)
        {
            Console.WriteLine("ChallengeUpdateModule constructor called");

            _handler = handler;
        }

        [SlashCommand("도전수정", "[리더 전용] 도전 임베드 내용을 수정합니다.")]
        [RequireCommandRole(Role.Leader)]
        public async Task ChallengeUpdate(
            [Summary("메시지ID", "수정할 도전 임베드 메시지의 ID입니다.")]
            int messageId
        )
        {

            var beforeMessage = await MessageUtil.GetMessageFromChannel(Context.Channel, Convert.ToUInt64(messageId));

            var modifyMb = new ModalBuilder()
                    .WithTitle("📌 도전 등록")
                    .WithCustomId("md_id_updateChallenge")
                    .AddTextInput("What??", "food_name", placeholder: "Pizza")
                    .AddTextInput(
                        "채널 이름 앞 `도전` 이 붙습니다. (띄어쓰기 - 기호 대체)",
                        "md_lb_updateChallenge_channelname",
                        placeholder: "채널명을 입력해주세요",
                        maxLength:45,
                        required:true,
                        value: "테스트"
                        );



            await Context.Interaction.RespondWithModalAsync<ChallengeUpdateModalContext>("md_id_updateChallenge");
        }
    }

    public class ChallengeUpdateModal : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BotConfig _config;
        private SqliteConnector _sqlite;

        public ChallengeUpdateModal(BotConfig config, SqliteConnector sqlite)
        {
            Console.WriteLine("ChallengeUpdateModalModule constructor called");

            _config = config;
            _sqlite = sqlite;
        }

        public class ChallengeUpdateModalContext : IModal
        {
            public string Title => "📌 도전 등록";

            // Strings with the ModalTextInput attribute will automatically become components.
            [InputLabel("채널 이름 앞 `도전` 이 붙습니다. (띄어쓰기 - 기호 대체)")]
            [RequiredInput(true)]
            [ModalTextInput("md_lb_updateChallenge_channelname", placeholder: "채널명을 입력해주세요", maxLength: 45)]
            public required string ChannelName { get; set; }

        }

        // Responds to the modal.
        [ModalInteraction("md_id_updateChallenge")]
        public async Task ModalResponse(ChallengeUpdateModalContext modal)
        {
            Console.WriteLine("dd");
        }

    }
}
