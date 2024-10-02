using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Data.Entities
{
    public class ChallengeEntity
    {
        public string Title { get; init; } = string.Empty;
        public ulong MessageId { get; init; } = ulong.MinValue;
        public ulong ChannelId { get; init; } = ulong.MinValue;
        public string LeaderName { get; init; } = string.Empty;
        public ulong LeaderId { get; init; } = ulong.MinValue;
    }
}
