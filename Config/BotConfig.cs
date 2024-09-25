using ExT.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Config
{
    public class BotConfig
    {
        public readonly string botName = "ExT";
        public readonly string botVersion = "0.0.4";

        // `ExT` Server
        public ulong guildID { get; private set; }
        // `운동 함께해요!` Category
        public ulong privateCategoryID { get; private set; }


        public BotConfig(ProgramMode environment)
        {
            guildID = (ulong)(environment == ProgramMode.Dev ? 1222901173200228583 : 1284028457830977617);
            privateCategoryID = (ulong)(environment == ProgramMode.Dev ? 1282607968650793010 : 1284044542479302656);
        }
    }
}
