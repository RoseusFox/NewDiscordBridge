using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus;

using TShockAPI;
using TShockAPI.DB;

namespace Terraria4PDA.DiscordBridge
{
    class Utils
    {
        static IDbConnection database;
        public Utils()
        {
            database = TShock.DB;
        }

        public static async void ReloadConfig()
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
            await Online();
            //Bridge.AutoBC();
        }

        public static async Task Online()
        {
            var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);
            await Discord.DiscordBot.SendMessageAsync(chat, $"✅ ***{TShock.Config.ServerName}***: Server online!");
        }
        public async void OnServerCommand(TerrariaApi.Server.CommandEventArgs args)
        {
            var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.LogID);

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
                TShock.Log.Error(ex.ToString());
            }
        }

        public static List<string> GetUserAccountByIP(string ip)
        {
            List<string> list = new List<string>();

            try
            {
                using (var reader = database.QueryReader($"SELECT Username FROM Users WHERE KnownIPs LIKE '%{ip}%';"))
                {
                    while (reader.Read())
                    {
                        //string[] words = reader.Get<string>("Username").Split(new char[] { ':' });
                        Console.WriteLine(ip + "\n" + reader.Get<string>("Username"));
                        list.Add(reader.Get<string>("Username"));
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.Error(ex.ToString());
            }

            return list;
        }
    }
}
