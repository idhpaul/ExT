using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Data.Entities
{
    public class DiscordUserEntity : IUserEntity
    {
        private readonly string _name = default!;
        public string Name { get { return _name; }}

        private readonly ulong _discordUserId = default;
        public ulong DiscordUserId { get { return _discordUserId; } }

        public DiscordUserEntity(string name, ulong discordUserId)
        {
            _name = name;
            _discordUserId = discordUserId;
        }

    }
}
