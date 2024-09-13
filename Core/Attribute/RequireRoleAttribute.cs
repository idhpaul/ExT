using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;
using ExT.Core.Enums;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Reflection;
using EnumsNET;
using Discord.WebSocket;

namespace ExT.Core.Attribute;


public class RequireCommandRole : PreconditionAttribute
{
    private readonly Role _roleName;

    public RequireCommandRole(Role roleName)
    {
        _roleName = roleName;
    }

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
    {
        var user = context.User as SocketGuildUser;
        if (user == null)
            return PreconditionResult.FromError("This command must be used in a guild.");

        //사용자가 특정 역할을 가지고 있는지 확인
        if (user.Roles.Any(role => role.Name == _roleName.AsString(EnumFormat.Description)))
            return PreconditionResult.FromSuccess();
        else
            return PreconditionResult.FromError($"`리더⚡`전용 명령어 입니다.");
    }

}
