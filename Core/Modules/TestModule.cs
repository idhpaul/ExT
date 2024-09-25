using Discord;
using Discord.Interactions;
using ExT.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExT.Core.Modules
{
    public class TestModule : InteractionModuleBase<SocketInteractionContext>
    {
        private InteractionHandler _handler;

        // Constructor injection is also a valid way to access the dependencies
        public TestModule(InteractionHandler handler)
        {
            Console.WriteLine("TestModule constructor called");

            _handler = handler;
        }

        [SlashCommand("ping", "Pings the bot and returns its latency.")]
        public async Task GreetUserAsync()
            => await RespondAsync(text: $":ping_pong: It took me {Context.Client.Latency}ms to respond to you!", ephemeral: true);

        [ComponentInteraction("foo")]
        public async Task ButtonPress()
        {
            await RespondAsync($"Test ComponentInteraction");
        }

    }
}
