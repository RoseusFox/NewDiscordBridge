using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using TerrariaApi.Server;
using Terraria;
using TShockAPI;
using TShockAPI.Hooks;
using TShockAPI.Localization;

using System.Data;
using TShockAPI.DB;

namespace Terraria4PDA.DiscordBridge
{
    class Logs
    {
        public static async void JoinLeave(TSPlayer player, bool join = true)
        {
            if (player == null)
                return;

            try
            {
                var ch = await Discord.DiscordBot.GetChannelAsync(Discord.Config.JoinLogID);
                string msg;
                DiscordEmoji emoji;

                if (join == false)
                {
                    emoji = DiscordEmoji.FromName(Discord.DiscordBot, ":x:");
                    msg = "** вышел.";
                }
                else
                {
                    emoji = DiscordEmoji.FromName(Discord.DiscordBot, ":arrow_right:");
                    msg = "** зашёл.";
                }

                await Discord.DiscordBot.SendMessageAsync(ch, emoji + " **" + player.Name + msg);
            }
            catch (Exception ex)
            {
                TShock.Log.Info("Пиздец тут:\n{");
                TShock.Log.Error(ex.ToString());
                TShock.Log.Info("}");
                TShock.Log.Warn(ex.Message);
            }

            return;
        }
        public async void Playing()
        {
            while (true)
            {
                var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.StatusID);
                var msg = chat.GetMessageAsync(Discord.Config.MsgID).Result;
                var players = new List<string>();

                foreach (TSPlayer ply in TShock.Players)
                {
                    if (ply != null && ply.Active)
                    {
                        players.Add(ply.Name);
                    }
                }
                var result = String.Join("`\n`", players.ToArray());

                var emoji = DiscordEmoji.FromName(Discord.DiscordBot, ":arrow_forward:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = $"***{TShock.Config.ServerName}***",
                    Description = $"✅ **Сервер онлайн!**\n\n{emoji} **Online Players:**\n{TShock.Utils.GetActivePlayerCount()} | {TShock.Config.MaxSlots}\n \n`{result}`",
                    Color = new DiscordColor(0x00fd2c) // green
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await msg.ModifyAsync("", embed: embed);

                //await msg.ModifyAsync("✅ **Сервер онлайн!**\n \n**Online Players:** \n" + TShock.Utils.GetActivePlayerCount() + " | " + TShock.Discord.Config.MaxSlots + "\n \n" + result);
                await Task.Delay(120000);
            }
        }

        public static async Task<bool> Goodbye()
        {
            var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);
            var status = await Discord.DiscordBot.GetChannelAsync(Discord.Config.StatusID);
            var msg = status.GetMessageAsync(Discord.Config.MsgID).Result;

            await Discord.DiscordBot.SendMessageAsync(chat, "🛠 **Сервер отключен!**");
            await Discord.DiscordBot.SendMessageAsync(chat, "🛠 **Server is down!**");

            //var emoji = DiscordEmoji.FromName(Discord.DiscordBot, ":no_entry:");
            //var embed = new DiscordEmbedBuilder
            //{
            //    Title = $"***{Discord.Config.Name}***",
            //    Description = $"{emoji} **Server offline!**",
            //    Color = new DiscordColor(0xff0000) // red
            //                                       // there are also some pre-defined colors available
            //                                       // as static members of the DiscordColor struct
            //};
            //await msg.ModifyAsync("", embed: embed);

            return true;
        }


        //public async void Hardmode(HandledEventArgs args)
        //{
        //    await Plugin4PDA.HardConfig();

        //    var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);
        //    await Discord.DiscordBot.SendMessageAsync(chat, "☠ Хардмод включен!");

        //    //List<int> itemIds = AntiCheat.Utils.GetIdItems(113);

        //    //if (itemIds.Count < 1)
        //    //    return;

        //    //foreach (int item in itemIds)
        //    //    TShock.Itembans.RemoveBan(EnglishLanguage.GetItemNameById(item));

        //    TSPlayer.All.SendInfoMessage("Хардмод предметы успешно разбанены!");

        //    return;
        //}

        private async Task HardmodeWipe()
        {
            while (true)
            {
                if (DateTime.Now.DayOfWeek != DayOfWeek.Saturday)
                {
                    DateTime today = DateTime.Today;
                    int daysUntil = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
                    //DateTime nextTuesday = today.AddDays(daysUntilTuesday);

                    TSPlayer.All.SendSuccessMessage((!Main.hardMode) ? $"{daysUntil} days left before hardmode" : $"{daysUntil} days left before wipe.");
                    TSPlayer.All.SendSuccessMessage((!Main.hardMode) ? $"До хардмода осталось {daysUntil}" : $"До вайпа осталось {daysUntil} days");
                }
                else
                {
                    if (DateTime.Now.Hour < 20)
                    {
                        int time = 20 - DateTime.Now.Hour;

                        TSPlayer.All.SendSuccessMessage((!Main.hardMode) ? $"{time} hours left before hardmode" : $"{time} hours left before wipe.");
                        TSPlayer.All.SendSuccessMessage((!Main.hardMode) ? $"До хардмода осталось {time}" : $"До вайпа осталось {time} hours");
                    }
                }

                await Task.Delay(1800000);
            }
        }


        public static async void OnServerCommand(CommandEventArgs args)
        {
            var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.LogID);
            if (args.Command.Contains("g 72") || args.Command.Contains("clear") || args.Command.Contains("login") || args.Command.Contains("user password"))
                return;
            await Discord.DiscordBot.SendMessageAsync(chat, "**Server:** " + args.Command);
        }
        public static async void OnPlayerCommand(PlayerCommandEventArgs args)
        {
            if ((args.Player == null) || (args.Player == TSPlayer.Server) || args.CommandText.Contains("login") || args.CommandText.Contains("user password"))
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


        public static async void CheatsDetected(TSPlayer player)
        {
            var cheats = await Discord.DiscordBot.GetChannelAsync(Discord.Config.CheatID);
            var guild = await Discord.DiscordBot.GetGuildAsync(565583751665549344);
            var staff = guild.GetRole(694425133489586237).Mention;
            var emojiplr = DiscordEmoji.FromName(Discord.DiscordBot, ":warning:");
            var embedplr = new DiscordEmbedBuilder
            {
                Title = "Потенциальный чит!",
                Description = $"{emojiplr} Игрок `{player.Name}` замечен в читерстве!",
                Color = new DiscordColor(0xFF0000) // red
                                                   // there are also some pre-defined colors available
                                                   // as static members of the DiscordColor struct
            };
            await Discord.DiscordBot.SendMessageAsync(cheats, staff, embed: embedplr);
        }
        public static async void PotentialCheats(TSPlayer player, int newMoney, int oldMoney)
        {
            DiscordChannel ch = await Discord.DiscordBot.GetChannelAsync(Discord.Config.CheatID);
            DiscordGuild guild = await Discord.DiscordBot.GetGuildAsync(565583751665549344);
            DiscordRole moder = guild.GetRole(676735113068937226);

            await Discord.DiscordBot.SendMessageAsync(ch, $"{moder.Mention}, потенциальный чит монет! `{player.Name}`. Старое значение платины - {oldMoney}, сейчас - {newMoney}.");
        }
    }
}
