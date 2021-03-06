﻿using Discord;
using MEE7.Backend;
using MEE7.Backend.HelperFunctions;
using System.Linq;

namespace MEE7.Commands
{
    public class Poll : Command
    {
        public readonly IEmote[] emotes = new IEmote[] { new Emoji("0⃣"), new Emoji("1⃣"), new Emoji("2⃣"), new Emoji("3⃣"), new Emoji("4⃣"),
                new Emoji("5⃣"), new Emoji("6⃣"), new Emoji("7⃣"), new Emoji("8⃣"), new Emoji("9⃣") };

        public Poll() : base("poll", "Creates a poll", false, false)
        {
            HelpMenu = new EmbedBuilder();
            HelpMenu.WithColor(0, 128, 255);
            HelpMenu.WithTitle("Poll");
            HelpMenu.WithDescription($"Write {PrefixAndCommand} [Your Question]\n[Answer1],[Anser2],...\n\neg.\n{PrefixAndCommand} Waddup?\n" +
                $"Nothing much,The Sky,My dick");
        }

        public override void Execute(IMessage message)
        {
            string[] split = message.Content.Split('\n');
            if (split.Length < 2)
            {
                DiscordNETWrapper.SendText($"Duuude no I need more Information, or different Information\nHave a look into my HelpMenu ({Prefix}help {CommandLine})",
                    message.Channel).Wait();
                return;
            }
            string[] pollOptions = split[1].Split(',');
            if (pollOptions.Length > 9)
            {
                DiscordNETWrapper.SendText("Duuude thats too many answer possibilities, 9 should be enough\nIf you want more ask the Discord Devs for a 10 Emoji",
                    message.Channel).Wait();
                return;
            }

            int i = 0;
            IUserMessage pollMessage = DiscordNETWrapper.SendEmbed(DiscordNETWrapper.CreateEmbedBuilder($":bar_chart: **{split[0].Remove(0, PrefixAndCommand.Length + 1)}**",
                pollOptions.Select(x => emotes[i++].Name + " " + x).Aggregate((x, y) => x + "\n" + y)),
                message.Channel).Result.First();
            pollMessage.AddReactionsAsync(emotes.Take(pollOptions.Length).ToArray()).Wait();

            return;
        }
    }
}
