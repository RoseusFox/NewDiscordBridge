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
    //public class HelpFormatter : IHelpFormatter
    //{
    //    private DiscordEmbedBuilder EmbedBuilder { get; }

    //    public HelpFormatter(CommandContext ctx) : base(ctx)
    //    {
    //        this.EmbedBuilder = new DiscordEmbedBuilder();
    //    }

    //    // this method is called first, it sets the current command's name
    //    // if no command is currently being processed, it won't be called
    //    public IHelpFormatter WithCommandName(string name)
    //    {
    //        string commandStr = null;

    //        foreach (var overload in command.Overloads)
    //        {
    //            if (overload.Arguments.Count == 0)
    //            {
    //                continue;
    //            }

    //            commandStr = Formatter.InlineCode(Program.Config.Prefix + command.Name + " " + string.Join(" ", overload.Arguments.Select(xarg => (xarg.IsOptional) ? $"[{xarg.Name}]" : $"<{xarg.Name}>")));
    //        }

    //        EmbedBuilder.AddField("Command", commandStr ?? Formatter.InlineCode($"{Program.Config.Prefix}{command.Name}"));
    //        EmbedBuilder.AddField("Description", Formatter.Italic(command.Description));

    //        return this;
    //    }

    //    // this method is called second, it sets the current command's 
    //    // description. if no command is currently being processed, it 
    //    // won't be called
    //    public IHelpFormatter WithDescription(string description)
    //    {
    //        this.MessageBuilder.Append("Description: ")
    //            .AppendLine(description)
    //            .AppendLine();

    //        return this;
    //    }

    //    // this method is called third, it is used when currently 
    //    // processed group can be executed as a standalone command, 
    //    // otherwise not called
    //    public IHelpFormatter WithGroupExecutable()
    //    {
    //        this.MessageBuilder.AppendLine("This group is a standalone command.")
    //            .AppendLine();

    //        return this;
    //    }

    //    // this method is called fourth, it sets the current command's 
    //    // aliases. if no command is currently being processed, it won't
    //    // be called
    //    public IHelpFormatter WithAliases(IEnumerable<string> aliases)
    //    {
    //        this.MessageBuilder.Append("Aliases: ")
    //            .AppendLine(string.Join(", ", aliases))
    //            .AppendLine();

    //        return this;
    //    }

    //    // this method is called fifth, it sets the current command's 
    //    // arguments. if no command is currently being processed, it won't 
    //    // be called
    //    public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
    //    {
    //        this.MessageBuilder.Append("Arguments: ")
    //            .AppendLine(string.Join(", ", arguments.Select(xarg => $"{xarg.Name} ({xarg.Type.ToUserFriendlyName()})")))
    //            .AppendLine();

    //        return this;
    //    }

    //    // this method is called sixth, it sets the current group's subcommands
    //    // if no group is being processed or current command is not a group, it 
    //    // won't be called
    //    public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
    //    {
    //        this.MessageBuilder.Append("Subcommands: ")
    //            .AppendLine(string.Join(", ", subcommands.Select(xc => xc.Name)))
    //            .AppendLine();

    //        return this;
    //    }

    //    // this is called as the last method, this should produce the final 
    //    // message, and return it
    //    public CommandHelpMessage Build()
    //    {
    //        return new CommandHelpMessage(this.MessageBuilder.ToString().Replace("\r\n", "\n"));
    //    }
    //}
}
