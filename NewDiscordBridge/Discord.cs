using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.EventArgs;

using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;

namespace Terraria4PDA.DiscordBridge
{
    [ApiVersion(2, 1)]
    class Discord : TerrariaPlugin
    {
        public override string Author => "Mortmael";

        public override string Description => "New Discord Bridge for Terraria 1.4";

        public override string Name => "Discord4PDA";

        public override Version Version => new Version(1, 0, 0, 0);

        public Discord(Main game) : base(game)
        {

        }

        public override async void Initialize()
        {
            #region Hooks
            ServerApi.Hooks.ServerCommand.Register(this, Logs.OnServerCommand);
            ServerApi.Hooks.GamePostInitialize.Register(this, Utils.OnPostInitialize);
            ServerApi.Hooks.ServerBroadcast.Register(this, Bridge.OnBC);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
            ServerApi.Hooks.WorldSave.Register(this, OnSave);

            GetDataHandlers.KillMe += Bridge.OnKill; 
            PlayerHooks.PlayerCommand += Logs.OnPlayerCommand;
            PlayerHooks.PlayerLogout += OnLogout;
            #endregion

            LoadConfig();

            if (Config.DiscordBotToken != "Token here")
            {
                DiscordBot = new DiscordClient(new DiscordConfiguration
                {
                    Token = Config.DiscordBotToken,
                    TokenType = TokenType.Bot,

                    UseInternalLogHandler = true,
                    LogLevel = LogLevel.Warning
                });
                await DiscordBot.ConnectAsync();

                if (Config.Commands)
                {
                    var ccfg = new CommandsNextConfiguration
                    {
                        StringPrefix = Config.Prefix,

                        EnableDms = false,

                        EnableMentionPrefix = false
                    };

                    this.DiscordCommands = DiscordBot.UseCommandsNext(ccfg);

                    DiscordCommands.RegisterCommands<BotCommands>();

                    this.DiscordCommands.SetHelpFormatter<HelpFormatter>();

                    this.DiscordCommands.CommandExecuted += this.Commands_CommandExecuted;
                    this.DiscordCommands.CommandErrored += this.Commands_CommandErrored;
                }

                DiscordBot.ClientErrored += this.Client_ClientError;
                DiscordBot.GuildMemberAdded += this.OnMemberJoin;
                DiscordBot.MessageCreated += OnMessageCreated;
            }
            else Environment.Exit(0);
        }

        protected override async void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.ServerCommand.Deregister(this, Logs.OnServerCommand);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, Utils.OnPostInitialize);
                ServerApi.Hooks.ServerBroadcast.Deregister(this, Bridge.OnBC);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreet);
                ServerApi.Hooks.WorldSave.Deregister(this, OnSave);

                GetDataHandlers.KillMe -= Bridge.OnKill;
                PlayerHooks.PlayerCommand -= Logs.OnPlayerCommand;
                PlayerHooks.PlayerLogout -= OnLogout;


                await Logs.Goodbye();
            }
            base.Dispose(disposing);
        }





        public static DiscordClient DiscordBot { get; set; }
        public CommandsNextModule DiscordCommands { get; set; }
        public static ConfigFile Config = new ConfigFile();
        //public ClanManager ClanManager = new ClanManager();

        public static void LoadConfig()
        {
            string path = Path.Combine(TShock.SavePath, "Configs", "Discord.json");
            Config = ConfigFile.Read(path);
        }



        #region Hooks
        private async Task OnMemberJoin(GuildMemberAddEventArgs args)
        {
            if (!Config.Welcomer)
                return;

            DiscordUser ilnur = await DiscordBot.GetUserAsync(419561144462606364);
            await args.Member.SendMessageAsync($"Hello! Welcome to ***{args.Guild.Name}***! Now we have two servers for MOBILE Terraria 1.3.0.7 and one PC server for latest 1.4. Please, read the rules! " +
                $"If you have a questions about technical issues, please contact the **{args.Guild.Owner.Username}**." +
                $"With other questions contact with **{ilnur.Username}**.");
            await args.Member.SendMessageAsync("**-----------------**");
            await args.Member.SendMessageAsync($"Добро пожаловать на ***{args.Guild.Name}***! Сейчас у нас есть два сервера для МОБИЛЬНОЙ Террарии 1.3.0.7 и один ПК сервер для последней 1.4. Пожалуйста, прочитайте правила!" +
                $"Если вы хотите задать технический вопрос, или указать на баг - пишите **{args.Guild.Owner.Username}**." +
                $"По другим вопросам пишите **{ilnur.Username}**");
            await args.Member.SendMessageAsync("**IP:** \n__Mobile 1__: `4pdaclub.sytes.net:7777`\n__Mobile 2__: `4pda-mg.sytes.net:7777`\n__PC (beta)__: `4pda-mg.sytes.net:7778`");

            var status = await DiscordBot.GetChannelAsync(Config.MemberCountID);
            var guild = await DiscordBot.GetGuildAsync(565583751665549344);

            int members = guild.MemberCount;

            await status.ModifyAsync("Members: " + members);
        }
        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Author == DiscordBot.CurrentUser)
                return;
            if (e.Author.IsBot)
                return;
            if (e.Channel.Id != Config.ChatID)
                return;

            DiscordMember member = await e.Guild.GetMemberAsync(e.Author.Id);
            string hex = "FFFFFF";

            foreach (ulong id in Config.Roles)
            {
                DiscordRole role = e.Guild.GetRole(id);
                if (member.Roles.Contains(role))
                {
                    System.Drawing.Color color = System.Drawing.Color.FromArgb(member.Color.R, member.Color.G, member.Color.B);
                    hex = color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
                    break;
                }
            }

            string text = $"[c/{hex}:" + e.Message.Content + "]";

            Regex regex = new Regex(@"\n");
            if (regex.IsMatch(e.Message.Content))
            {
                string[] strings = regex.Split(e.Message.Content);
                text = "";
                foreach (string str in strings)
                {
                    if (string.IsNullOrWhiteSpace(str))
                        continue;
                    text += $"[c/{hex}:" + str + "]\n";
                }
            }

            TShock.Utils.Broadcast(String.Format(Config.MessageFormatDiscordToTerraria, e.Author.Username, @text), Config.GetColor());
            return;
        }


        private Task Client_ClientError(ClientErrorEventArgs e)
        {
            // let's log the details of the error that just 
            // occured in our client
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}\n \n{e.Exception.InnerException}", DateTime.Now);

            TShock.Log.Error(e.Exception.Source + ": \n" + e.Exception.Message + "\n \n" + e.Exception.InnerException);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return Task.CompletedTask;
        }

        private async Task Commands_CommandExecuted(CommandExecutionEventArgs e)
        {
            // let's log the name of the command and user
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, "ExampleBot", $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);

            var logs = await DiscordBot.GetChannelAsync(Config.LogID);
            await DiscordBot.SendMessageAsync(logs, "**" + e.Context.User.Username + ":** " + e.Context.Message.Content);

            // since this method is not async, let's return
            // a completed task, so that no additional work
            // is done
            return;
        }
        private async Task Commands_CommandErrored(CommandErrorEventArgs e)
        {
            e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, "ExampleBot", $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}\n \n{e.Exception.InnerException}", DateTime.Now);

            TShock.Log.Error(e.Exception.Source + ": \n" + e.Exception.Message + "\n \n" + e.Exception.InnerException);

            if (e.Exception is ChecksFailedException ex)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

                var embed = new DiscordEmbedBuilder
                {
                    Title = "Access denied",
                    Description = $"{emoji} You do not have the permissions required to execute this command.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await e.Context.RespondAsync("", embed: embed);
            }
            if (e.Exception is ArgumentException)
            {
                var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Invalid command syntax",
                    Description = $"{emoji} Type `/help {e.Command.Name}`",
                    Color = new DiscordColor(0xFF0000) // red
                    // there are also some pre-defined colors available
                    // as static members of the DiscordColor struct
                };
                await e.Context.RespondAsync("", embed: embed);
            }
        }

        static void OnGreet(GreetPlayerEventArgs args)
        {
            if (Config.Chat)
            {
                Logs.JoinLeave(TShock.Players[args.Who]);
            }

            Utils.CountPlayers();
        }
        static void OnLogout(PlayerLogoutEventArgs args)
        {
            if (args.Player == null)
            {
                return;
            }


            Utils.CountPlayers();

            if (args.Player.ReceivedInfo)
            {
                Logs.JoinLeave(args.Player, false);
            }
            return;
        }
        void OnSave(WorldSaveEventArgs args)
        {
            if (Config.Chat)
            {
                Bridge.OnSaveWorld(args);
            }
        }
        private void OnChat(PlayerChatEventArgs args)
        {
            if (Config.Chat)
            {
                Bridge.SendMessage(args);
            }
        }
        #endregion
    }
}
