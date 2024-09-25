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
using static ExT.Core.Modules.RegistExerciseModal;
using ExT.Core.Enums;
using EnumsNET;
using ExT.Config;

namespace ExT.Core.Modules
{
    public class RegistExerciseModule : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionHandler _handler;

        public RegistExerciseModule(InteractionHandler handler)
        {
            Console.WriteLine("RegistExerciseModule constructor called");

            _handler = handler;
        }

        [SlashCommand("도전등록", "[리더 전용] 운동 채널을 생성합니다.")]
        [RequireCommandRole(Role.Leader)]
        public async Task RegistExercise()
        {
            await Context.Interaction.RespondWithModalAsync<RegistExerciseModalContext>("md_id_regExercise");
        }
    }

    public class RegistExerciseModal : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly BotConfig _config;

        public RegistExerciseModal(BotConfig config)
        {
            Console.WriteLine("RegistExerciseModalModule constructor called");

            _config = config;
        }

        public class RegistExerciseModalContext : IModal
        {
            public string Title => "📌 도전 등록";

            // Strings with the ModalTextInput attribute will automatically become components.
            [InputLabel("채널 이름 앞 `도전` 이 붙습니다. (띄어쓰기 - 기호 대체)")]
            [RequiredInput(true)]
            [ModalTextInput("md_lb_regExercise_channelname", placeholder: "채널명을 입력해주세요", maxLength: 45)]
            public required string ChannelName { get; set; }

            // Additional paremeters can be specified to further customize the input.    
            // Parameters can be optional
            //[RequiredInput(false)]
            //[InputLabel("Why??")]
            //[ModalTextInput("food_reason", TextInputStyle.Paragraph, "Kuz it's tasty", maxLength: 500)]
            //public string Reason { get; set; }
        }

        // Responds to the modal.
        [ModalInteraction("md_id_regExercise")]
        public async Task ModalResponse(RegistExerciseModalContext modal)
        {
            // 채널 중복 확인

            // 채널 생성
            var guild = Context.Guild;
            var developerRole = guild.Roles.FirstOrDefault(r => r.Name == Role.Developer.AsString(EnumFormat.Description));
            if (developerRole is null)
            {
                await RespondAsync("`Developer🚀` 역할을 찾을 수 없습니다. 설정을 확인해주세요.", ephemeral:true);
                return; // 메서드 실행 중단
            }

            var developerRoleId = developerRole.Id;

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

            await privateChannel.SendMessageAsync($"# 💪 채널 이용 방법\r\n" +
                $"> 🔸 이 채널은 도전에 참가한 사람들에게만 보여집니다.\r\n" +
                $"> 🔸 채널에 사진을 업로드하면 `분석` 후 `요약 결과` 를 볼 수 있습니다.\r\n" +
                $"> 🔸 각자 업로드한 사진은 쓰레드에 보관됩니다.\r\n" +
                $"> 🔸 도전 참가자들 간 자유로운 대화 가능합니다.\r\n" +
                $"> 🔸 상호 간 존중 및 예의를 지켜주세요.");

            // 임베드 
            var embed = new EmbedBuilder()
                            .WithTitle(modal.ChannelName)
                            .WithDescription("임베드 설명")
                            .WithColor(Color.Blue) // 색상 설정
                            .WithFooter("하단 메시지") // 하단 메시지 설정
                            .WithTimestamp(DateTimeOffset.Now) // 타임스탬프 설정
                            .Build();

            // 버튼 생성
            var buttons = new ComponentBuilder()
                            .WithButton("Join", $"bt_join_{privateChannel.Id}", ButtonStyle.Primary)
                            //.WithButton("Detail", "bt_detail", ButtonStyle.Secondary)
                            .Build();

            // Check if "Why??" field is populated
            string channelName = string.IsNullOrWhiteSpace(modal.ChannelName)
                ? "."
                : $" because {modal.ChannelName}";

            // Build the message to send.
            string message = "create :  " +
                $"{modal.ChannelName}";


            // Respond to the modal.
            await RespondAsync(embed: embed, components: buttons);
            await RespondAsync(message, ephemeral: true);
        }

    }
}
