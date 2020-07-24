using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace Terraria4PDA.DiscordBridge
{
    public class HelpFormatter : IHelpFormatter
    {
        private DiscordEmbedBuilder EmbedBuilder { get; }
        private string commandStr;
        private bool Args = false;

        public HelpFormatter()
        {
            this.EmbedBuilder = new DiscordEmbedBuilder();
        }

        public IHelpFormatter WithCommandName(string name)
        {
            commandStr = Discord.Config.Prefix + name;

            return this;
        }

        // this method is called second, it sets the current command's 
        // description. if no command is currently being processed, it 
        // won't be called
        public IHelpFormatter WithDescription(string description)
        {
            EmbedBuilder.AddField("Description", Formatter.Italic(description));

            return this;
        }

        // this method is called third, it is used when currently 
        // processed group can be executed as a standalone command, 
        // otherwise not called
        public IHelpFormatter WithGroupExecutable()
        {
            EmbedBuilder.Description = "This group is a standalone command.";

            return this;
        }

        // this method is called fourth, it sets the current command's 
        // aliases. if no command is currently being processed, it won't
        // be called
        public IHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            EmbedBuilder.AddField("Aliases", string.Join(", ", aliases));

            return this;
        }

        // this method is called fifth, it sets the current command's 
        // arguments. if no command is currently being processed, it won't 
        // be called
        public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            Args = true;
            commandStr += " " + string.Join(" ", arguments.Select(xarg => (xarg.IsOptional) ? $"[{xarg.Name}]" : $"<{xarg.Name}>"));

            EmbedBuilder.AddField("Command", commandStr);

            return this;
        }

        // this method is called sixth, it sets the current group's subcommands
        // if no group is being processed or current command is not a group, it 
        // won't be called
        public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            EmbedBuilder.AddField("Commands", string.Join(", ", subcommands.Select(xc => Formatter.InlineCode(xc.Name))));

            return this;
        }

        // this is called as the last method, this should produce the final 
        // message, and return it
        public CommandHelpMessage Build()
        {
            EmbedBuilder.Title = "HELP";
            EmbedBuilder.ThumbnailUrl = Discord.DiscordBot.CurrentUser.AvatarUrl;
            EmbedBuilder.WithFooter("Type /help <command>", Discord.DiscordBot.CurrentUser.AvatarUrl);
            EmbedBuilder.Timestamp = DateTime.UtcNow;

            return new CommandHelpMessage(null, EmbedBuilder.Build());
        }
    }
}
