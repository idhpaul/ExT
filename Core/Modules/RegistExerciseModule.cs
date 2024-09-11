using Discord;
using Discord.Interactions;
using ExT.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ExT.Core.Modules.ModalModule;

namespace ExT.Core.Modules
{
    public class RegistExerciseModule : InteractionModuleBase<SocketInteractionContext>
    {
        // Dependencies can be accessed through Property injection, public properties with public setters will be set by the service provider
        public InteractionService Commands { get; set; }

        private InteractionHandler _handler;

        // Constructor injection is also a valid way to access the dependencies
        public RegistExerciseModule(InteractionHandler handler)
        {
            Console.WriteLine("RegistExerciseModule constructor called");

            _handler = handler;
        }

        [SlashCommand("운동등록", "[리더 전용] 운동 채널을 생성합니다.")]
        public async Task RegistExercise()
        {
            await Context.Interaction.RespondWithModalAsync<RegistExerciseModal>("md_id_regExercise");
        }
    }

    public class ModalModule : InteractionModuleBase<SocketInteractionContext>
    {
        public class RegistExerciseModal : IModal
        {
            public string Title => "📌 운동 등록";

            // Strings with the ModalTextInput attribute will automatically become components.
            [InputLabel("채널 제목 (\"띄어쓰기의 경우 - 기호로 대체됩니다\"")]
            [RequiredInput(true)]
            [ModalTextInput("md_lb_regExercise_channelname", placeholder: "채널명을 입력해주세요", maxLength: 30)]
            public string ChannelName { get; set; }

            // Additional paremeters can be specified to further customize the input.    
            // Parameters can be optional
            //[RequiredInput(false)]
            //[InputLabel("Why??")]
            //[ModalTextInput("food_reason", TextInputStyle.Paragraph, "Kuz it's tasty", maxLength: 500)]
            //public string Reason { get; set; }
        }

        // Responds to the modal.
        [ModalInteraction("md_id_regExercise")]
        public async Task ModalResponse(RegistExerciseModal modal)
        {
            // 채널 중복 확인

            // 채널 생성
            var guild = Context.Guild;

            var newChannel = await guild.CreateTextChannelAsync(modal.ChannelName, properties =>
            {
                properties.CategoryId = 1282607968650793010; // 카테고리 ID
                properties.Topic = $"{modal.ChannelName} 채널입니다.";
            });

            // 임베드 
            var embed = new EmbedBuilder()
                            .WithTitle("임베드 제목")
                            .WithDescription("임베드 설명")
                            .WithColor(Color.Blue) // 색상 설정
                            .WithFooter("하단 메시지") // 하단 메시지 설정
                            .WithTimestamp(DateTimeOffset.Now) // 타임스탬프 설정
                            .Build();

            // 버튼 생성
            var buttons = new ComponentBuilder()
                            .WithButton("Join", "bt_join", ButtonStyle.Primary)
                            .WithButton("Detail", "bt_detail", ButtonStyle.Secondary)
                            .Build();

            // Check if "Why??" field is populated
            string channelName = string.IsNullOrWhiteSpace(modal.ChannelName)
                ? "."
                : $" because {modal.ChannelName}";

            // Build the message to send.
            string message = "create :  " +
                $"{modal.ChannelName}";

            // Specify the AllowedMentions so we don't actually ping everyone.
            //AllowedMentions mentions = new();
            //mentions.AllowedTypes = AllowedMentionTypes.Users;

            // Respond to the modal.
            await RespondAsync(embed: embed, components: buttons);
            await RespondAsync(message, ephemeral: true);
        }
    }
}
