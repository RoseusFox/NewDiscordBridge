using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using TerrariaApi.Server;

using TShockAPI;
using TShockAPI.Hooks;

namespace Terraria4PDA.DiscordBridge
{
    class Bridge
    {
        public static async void OnBC(ServerBroadcastEventArgs args)
        {
            try
            {
                string msg = args.Message.ToString();
                var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);

                try
                {
                    Regex regex = new Regex(@"\[i:(\S*)]");
                    MatchCollection matches = regex.Matches(msg);

                    if (matches.Count > 0)
                        msg = regex.Replace(msg, "");

                    Regex isPlayer = new Regex(@".*:");
                    Regex regex1 = new Regex(@".* was .*kicked for '.*'");
                    Regex regex2 = new Regex(@".* .*kicked .* for '.*'");

                    if (msg.Contains("(Server Broadcast)") || regex1.IsMatch(msg) || regex2.IsMatch(msg))
                    {
                        await Discord.DiscordBot.SendMessageAsync(chat, $"*{msg}*");
                    }
                }
                catch (Exception a)
                {
                    TShock.Log.ConsoleError("DiscordBridge error when sending message to discord: {0}", a.Message);
                }
            }
            catch (RateLimitException ex) { }

            return;
        }
        public static async void SendMessage(PlayerChatEventArgs args = null, string message = "")
        {
            if (message == "" && args != null)
            {
                if (string.IsNullOrWhiteSpace(args.RawText))
                    return;

                string prefix = "**" + args.Player.Group.Prefix;
                string suffix = args.Player.Group.Suffix + "**";
                string name = args.Player.Name;

                string text = String.Format(TShock.Config.ChatFormat, args.Player.Group.Name, prefix, name, suffix,
                                                 args.RawText);

                try
                {
                    var channel = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);
                    Regex regex = new Regex(@"\[i:(\w*)]");
                    Regex regexWamount = new Regex(@"\[i[/s](\w*):(\w*)]");
                    //MatchCollection matches = ;
                    if (regex.Matches(text).Count > 0 || regex.Matches(text).Count > 0)
                    {
                        //foreach (Match match in matches) 
                        //{
                        //    Match nums = Regex.Match(match.Value, @"\d{1,}");

                        //}
                        string newstr = regex.Replace(text, "");
                        newstr = regexWamount.Replace(newstr, "");
                        await Discord.DiscordBot.SendMessageAsync(channel, newstr);
                    }
                    else
                    {
                        await Discord.DiscordBot.SendMessageAsync(channel, text);
                    }
                }
                catch (Exception a)
                {
                    TShock.Log.ConsoleError("DiscordBridge error when sending message to discord: {0}", a.Message);
                }
            }
            else
            {
                var ch = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);
                await Discord.DiscordBot.SendMessageAsync(ch, message);
            }
        }
        public static void OnKill(object sender, GetDataHandlers.KillMeEventArgs args)
        {
            SendMessage(null, "*" + args.PlayerDeathReason + "*"); //.GetDeathText(args.Player.Name).ToString()

            IEnumerable<TSPlayer> alive = from ply in TShock.Players
                                          where !ply.Dead && ply != null && ply.Name != ""
                                          select ply;
            if (TShock.Utils.GetActivePlayerCount() > 0 && Plugin4PDA.Bosses.Count() > 0)
            {
                if (!alive.Any())
                {
                    string bosses = "";
                    foreach (Plugin4PDA.BossFight boss in Plugin4PDA.Bosses)
                    {
                        if (bosses == "")
                            bosses = boss.Boss.FullName;
                        else
                            bosses += ", " + boss.Boss.FullName;
                    }

                    Plugin4PDA.Bosses.Clear();

                    TSPlayer.All.SendErrorMessage("Последний игрок(" + args.Player.Name + ") погиб и босс(ы) " + bosses + " улетел(и)!");
                    SendMessage(null, "***Последний игрок (" + args.Player.Name + ") погиб и босс(ы) __" + bosses + "__ улетел(и)!***"); //.GetDeathText(args.Player.Name).ToString()
                }
            }
        }
        public static async void OnSaveWorld(WorldSaveEventArgs args)
        {
            if (TShock.Config.AnnounceSave)
            {
                // Protect against internal errors causing save failures
                // These can be caused by an unexpected error such as a bad or out of date plugin
                try
                {
                    var ch = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);
                    await Discord.DiscordBot.SendMessageAsync(ch, "*Saving world...*");
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("World saved notification failed");
                    TShock.Log.Error(ex.ToString());
                }
            }
        }


        public static async Task AutoBC()
        {
            var chat = await Discord.DiscordBot.GetChannelAsync(Discord.Config.ChatID);
            Warn(chat);
            //HardmodeWipe();
        }
        private static async Task Warn(DiscordChannel chat)
        {
            await Discord.DiscordBot.SendMessageAsync(chat, "**ВНИМАНИЕ!** Этот чат предназначен для общения непосредственно с игроками в игре. Всё остальное общение переносим в другие чаты!\n**ATTENTION!** This chat is for communicate directly with the players in the game. All other communication is transferred to other chats!");
            while (true)
            {
                if (TShock.Utils.GetActivePlayerCount() > 3)
                    await Discord.DiscordBot.SendMessageAsync(chat, "**ВНИМАНИЕ!** Этот чат предназначен для общения непосредственно с игроками в игре. Всё остальное общение переносим в другие чаты!\n**ATTENTION!** This chat is for communicate directly with the players in the game. All other communication is transferred to other chats!");
                await Task.Delay(1200000);
            }
        }
    }
}
