﻿using ExT.Core.Enums;
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
        public readonly string botVersion = "0.0.10-beta";

        public readonly string botDbName = default!;
        public readonly string botDbPath = default!;
        public readonly string botDbConnectionString = default!;

        // `ExT` Server
        public readonly ulong guildID = default!;
        // `운동 함께해요!` Category
        public readonly ulong privateCategoryID = default!;


        public BotConfig(ProgramMode environment)
        {
            guildID = (ulong)(environment == ProgramMode.Dev ? 1222901173200228583 : 1284028457830977617);
            privateCategoryID = (ulong)(environment == ProgramMode.Dev ? 1282607968650793010 : 1284044542479302656);

            botDbName = environment == ProgramMode.Dev
                ? "ExT_dev.sqlite" : "ExT.sqlite";
            botDbPath = environment == ProgramMode.Dev
                ? $"{Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName, botDbName)}"
                : $"{Path.Combine(Environment.CurrentDirectory, botDbName)}";
            botDbConnectionString = $"Data Source={botDbPath}";
        }
    }
}
