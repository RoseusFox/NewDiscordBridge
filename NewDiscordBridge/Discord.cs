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

using Microsoft.Xna.Framework;

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

        static Color Color;

        public override async void Initialize()
        {
            #region Hooks
            ServerApi.Hooks.ServerCommand.Register(this, Logs.OnServerCommand);
            ServerApi.Hooks.GamePostInitialize.Register(this, Utils.OnPostInitialize);
            ServerApi.Hooks.ServerBroadcast.Register(this, Bridge.OnBC);
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreet);
            ServerApi.Hooks.WorldSave.Register(this, OnSave);

            GeneralHooks.ReloadEvent += OnReload;
            //GetDataHandlers.KillMe += Bridge.OnKill; 
            PlayerHooks.PlayerCommand += Logs.OnPlayerCommand;
            PlayerHooks.PlayerLogout += OnLogout;
            PlayerHooks.PlayerChat += OnChat;
            #endregion

            LoadConfig();

            Color = new Color(Config.Messagecolor[0], Config.Messagecolor[1], Config.Messagecolor[2]);

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

                    //this.DiscordCommands.SetHelpFormatter<HelpFormatter>();

                    this.DiscordCommands.CommandExecuted += this.CommandExecuted;
                    this.DiscordCommands.CommandErrored += this.CommandErrored;
                }

                DiscordBot.ClientErrored += this.ClientErrored;
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

                GeneralHooks.ReloadEvent -= OnReload;
                //GetDataHandlers.KillMe -= Bridge.OnKill;
                PlayerHooks.PlayerCommand -= Logs.OnPlayerCommand;
                PlayerHooks.PlayerLogout -= OnLogout;
                PlayerHooks.PlayerChat -= OnChat;


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
            string path = Path.Combine(TShock.SavePath, "NewDiscordBridge.json");
            Config = ConfigFile.Read(path);
        }



        #region SysHooks
        private Task ClientErrored(ClientErrorEventArgs e)
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

        private async Task CommandExecuted(CommandExecutionEventArgs e)
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
        private async Task CommandErrored(CommandErrorEventArgs e)
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
        #endregion

        #region Discord Hooks
        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            if (e.Channel.Id != Config.ChatID)
                return;
            if (e.Author == DiscordBot.CurrentUser || e.Author.IsBot)
                return;

            TShock.Utils.Broadcast(string.Format(Config.DiscordToTerrariaFormat, e.Author.Username, e.Message.Content), Color);
            return;
        }
        #endregion

        #region Terraria Hooks
        void OnReload(ReloadEventArgs args)
        {
            Utils.ReloadConfig();
            args.Player.SendSuccessMessage("NewDiscordBridge config reloaded.");
        }
        void OnGreet(GreetPlayerEventArgs args)
        {
            if (Config.JoinLogID != 0)
            {
                Logs.JoinLeave(TShock.Players[args.Who]);
            }
        }
        void OnLogout(PlayerLogoutEventArgs args)
        {
            if (args.Player == null)
                return;

            if (args.Player.ReceivedInfo)
                Logs.JoinLeave(args.Player, false);

            return;
        }
        void OnSave(WorldSaveEventArgs args)
        {
            if (Config.Chat)
            {
                Bridge.OnSaveWorld(args);
            }
        }
        void OnChat(PlayerChatEventArgs args)
        {
            if (Config.Chat)
            {
                if (string.IsNullOrWhiteSpace(args.RawText))
                    return;

                string msg = string.Format(Discord.Config.TerrariaToDiscordFormat,
                    args.Player.Group.Prefix,
                    args.Player.Name,
                    args.Player.Group.Suffix,
                    args.RawText);

                Bridge.SendMessage(msg);
            }
        }
        #endregion
    }
}
