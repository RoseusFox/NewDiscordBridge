using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;

using TShockAPI;

namespace Terraria4PDA.DiscordBridge
{
    class Utils
    {
        public async void ReloadConfig()
        {
            await Discord.DiscordBot.DisconnectAsync();

            Discord.DiscordBot = new DiscordClient(new DiscordConfiguration
            {
                Token = Discord.Config.DiscordBotToken,
                TokenType = TokenType.Bot,

                UseInternalLogHandler = true,
                LogLevel = LogLevel.Warning
            });

            await Discord.DiscordBot.ConnectAsync();
        }

        public static async void OnPostInitialize(EventArgs args)
        {
            Logs Tasks = new Logs();
            await Online();
            Bridge.AutoBC();
            //Console.WriteLine("Logged in as");
            //Console.WriteLine(Discord.DiscordBot.CurrentUser.Username);
            //Console.WriteLine("\nVersion:");
            //Console.WriteLine(Discord.DiscordBot.VersionString);
            //Console.WriteLine("--------------");
        }

        public static async Task Online()
        {
            var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);
            await Discord.DiscordBot.SendMessageAsync(chat, $"✅ ***{TShock.Config.ServerName}***: Сервер онлайн!");
            await Discord.DiscordBot.SendMessageAsync(chat, $"✅ ***{TShock.Config.ServerName}***: Server online!");
            //Playing();
        }
        public async void OnServerCommand(TerrariaApi.Server.CommandEventArgs args)
        {
            var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.LogID);
            if (args.Command.Contains("g 72") || args.Command.Contains("clear"))
                return;
            await Discord.DiscordBot.SendMessageAsync(chat, "**Server:** " + args.Command);
        }
        public async void OnPlayerCommand(TShockAPI.Hooks.PlayerCommandEventArgs args)
        {
            if ((args.Player == null) || (args.Player == TSPlayer.Server))
                return;
            try
            {
                var logs = await Discord.DiscordBot.GetChannelAsync(Discord.Config.LogID);
                await Discord.DiscordBot.SendMessageAsync(logs, "**" + args.Player.Name + ":** " + args.CommandText);
            }
            catch (Exception ex)
            {
                TShock.Log.Info("Пиздец тут:\n{");
                TShock.Log.Error(ex.ToString());
                TShock.Log.Info("}");
                TShock.Log.Warn(ex.Message);
            }
        }
    }
}
