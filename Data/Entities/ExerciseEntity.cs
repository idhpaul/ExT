using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Data.Entities
{
    public class ExerciseEntity
    {
        public string ExerciseTime { get; set; } = string.Empty;
        public string CaloriesBurned { get; set; } = string.Empty;
        public string OtherData { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;
        public ulong UserId { get; init; } = ulong.MinValue;
        public ulong ChannelId { get; init; } = ulong.MinValue;
    }
}
