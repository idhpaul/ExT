using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Data.Entities
{
    public class DiscordUserEntity : IUserEntity
    {
        private string _name = default!;
        public string Name { get { return _name; } set { _name = value; } }

        private ulong _discordUserId = default;
        public ulong DiscordUserId { get { return _discordUserId; } set { _discordUserId = value; } }

    }
}
