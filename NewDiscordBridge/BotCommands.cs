using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Terraria;
using TShockAPI.Localization;
using TShockAPI;
using TShockAPI.DB;
using Terraria.Localization;
using Microsoft.Xna.Framework;

namespace Terraria4PDA.DiscordBridge
{
    [RequireRolesAttribute("Server Owner")] // let's mark this class as a command group
    [Description("Owner commands.")] // give it a description for help purposes
    public class OwnerBotCommands
    {
        //[Command("wipe")]
        //[Description("Производит вайп")]
        //public async Task Wipe(CommandContext ctx)
        //{
        //    if (ctx.Channel.Id != 565744641677197323)
        //        return;

        //    string path = Path.Combine(TShock.SavePath, "config.json");
        //    string json = File.ReadAllText(path);
        //    dynamic jsonObj = JsonConvert.DeserializeObject(json);
        //    jsonObj["DisableHardmode"] = true;
        //    string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
        //    File.WriteAllText(path, output);

        //    Main.hardMode = false;
        //    TShock.Config.DisableHardmode = true;


        //    int[] npcIds = { 4, 13, 35, 50, 125, 126, 127, 134, 222, 245, 262, 266, 370, 439, 440, 398 };

        //    foreach (int boss in npcIds)
        //    {
        //        List<int> itemIds = AntiCheat.Utils.GetIdItems(boss);

        //        if (itemIds.Count < 1)
        //            continue;

        //        foreach (int item in itemIds)
        //            TShock.Itembans.AddNewBan(EnglishLanguage.GetItemNameById(item));
        //        Console.WriteLine(TShock.Utils.GetNPCById(boss).FullName);
        //    }
        //    Console.WriteLine("Итембаны успешно восстановлены!");

        //    await Plugin4PDA.HardConfig(false);

        //    var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
        //    var embed = new DiscordEmbedBuilder
        //    {
        //        Title = "Готово!",
        //        Description = $"{emoji} Успешно!",
        //        Color = new DiscordColor(0x00fd2c) // green
        //                                           // there are also some pre-defined colors available
        //                                           // as static members of the DiscordColor struct
        //    };
        //    await ctx.RespondAsync("", embed: embed);
        //    return;
        //}
        [Command("off")]
        [Description("Выключение")]
        public async Task Off(CommandContext ctx)
        {
            await ctx.RespondAsync("Выключение...");

            if (Main.ServerSideCharacter)
            {
                foreach (TSPlayer player in TShock.Players)
                {
                    if (player != null && player.IsLoggedIn && !player.IsDisabledPendingTrashRemoval)
                    {
                        player.SaveServerCharacter();
                    }
                }
            }

            Plugin4PDA.LegalStop();
            TShock.Utils.StopServer(true, "Server shutting down!");
        }
    }

    [RequireRolesAttribute("Server Owner", "Server Administrator", "Server Super Moderator")] // let's mark this class as a command group
    [Description("Administrative commands.")] // give it a description for help purposes
    public class AdminBotCommands
    {
        //public DiscordClient DiscordBot { get; set; }
        
        [Command("ban")]
        [Description("/ban <user> <time> <reason>")]
        public async Task Ban(CommandContext ctx, string user, string time, string reason)
        {
            //var log = await DiscordBot.GetChannelAsync(565744641677197323);
            //await DiscordBot.SendMessageAsync(log, $"**{ctx.Member.Username}:** `ban`");

            bool parsedOkay = false;
            bool success = false;
            string targetGeneralizedName = "";
            int banLengthInSeconds = 0;

            List<TSPlayer> players = TSPlayer.FindByNameOrID(user);
            UserAccount offlineUserAccount = TShock.UserAccounts.GetUserAccountByName(user);

            if (time != "0")
			{
				parsedOkay = TShock.Utils.TryParseTime(time, out banLengthInSeconds);
			}
			else
			{
				parsedOkay = true;
			}

			if (!parsedOkay)
			{
                var emojitime = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedtime = new DiscordEmbedBuilder
                {
                    Title = "Неправильный формат времени",
                    Description = $"{emojitime} Допустимые значения: *0 (перманент), Xs, Xm, Xh, Xd*",
                    Color = new DiscordColor(0xFF0000) // red
                    // there are also some pre-defined colors available
                    // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embedtime);
                return;
            }

            if (players.Count == 1)
            {
                TSPlayer target = players[0];
                if (target.HasPermission(TShockAPI.Permissions.immunetoban))
                {
                    var emojiimmune = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embedimmune = new DiscordEmbedBuilder
                    {
                        Title = "Недостаточно прав!",
                        Description = $"{emojiimmune} Данный игрок имеет иммунитет к бану!",
                        Color = new DiscordColor(0xFF0000) // red
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embedimmune);
                    return;
                }

                targetGeneralizedName = target.Name;
                success = TShock.Bans.AddBan(target.IP, target.Name, target.UUID, target.Account?.Name ?? "", reason, false, ctx.User.Username + "#" + ctx.User.Discriminator,
                    banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

                Terraria4PDA.CommandManager.AdvancedBan.AddBanToDB(target.IP, target.Name, target.UUID, target.Account?.Name ?? "", reason, ctx.User.Username + "#" + ctx.User.Discriminator,
                    banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

                if (success)
                {
                    if (banLengthInSeconds == 0)
                    {
                        target.Disconnect(String.Format("Permanently banned for {0}", reason));
                    }
                    else
                    {
                        target.Disconnect(String.Format("Banned for {0} seconds for {1}", banLengthInSeconds, reason));
                    }
                }
            }
            if (players.Count == 0)
            {
                // If the target is a valid IP...
                string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
                Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
                if (r.IsMatch(user))
                {
                    targetGeneralizedName = "IP: " + user;
                    success = TShock.Bans.AddBan(user, "", "", "", reason,
                        false, ctx.User.Username, banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

                    CommandManager.AdvancedBan.AddBanToDB(user, "", "", "", reason,
                        ctx.User.Username, banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));


                    if (success && offlineUserAccount != null)
                    {
                        var emojiip = DiscordEmoji.FromName(ctx.Client, ":b:");
                        var embedip = new DiscordEmbedBuilder
                        {
                            Title = $"{emojiip} IP успешно забанен!",
                            Description = $"**IP:** `{user}`\n**Time:** `{time}`\n**Reason:** `{reason}`\n \n ВНИМАНИЕ! По этому IP был найден аккаунт, он также забанен!*",
                            Color = new DiscordColor(0x00fd2c) // green
                                                               // there are also some pre-defined colors available
                                                               // as static members of the DiscordColor struct
                        };
                        await ctx.RespondAsync("", embed: embedip);
                    }
                }
                else
                {
                    // Apparently there is no way to not IP ban someone
                    // This means that where we would normally just ban a "character name" here
                    // We can't because it requires some IP as a primary key.
                    if (offlineUserAccount == null)
                    {
                        var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                        var embederror = new DiscordEmbedBuilder
                        {
                            Title = "Невозможно забанить пользователя!",
                            Description = $"{emojierror} По запросу `{user}` не найдено ни одного игрока, аккаунта, или IP!",
                            Color = new DiscordColor(0xFF0000) // red
                                                               // there are also some pre-defined colors available
                                                               // as static members of the DiscordColor struct
                        };
                        await ctx.RespondAsync("", embed: embederror);
                        return;
                    }
                }

            }
            if (players.Count == 0 && offlineUserAccount != null)
            {
                // Catch: we don't know an offline player's last login character name
                // This means that we're banning their *user name* on the assumption that
                // user name == character name
                // (which may not be true)
                // This needs to be fixed in a future implementation.
                targetGeneralizedName = offlineUserAccount.Name;

                if (TShock.Groups.GetGroupByName(offlineUserAccount.Group).HasPermission(TShockAPI.Permissions.immunetoban))
                {
                    var emojiimmune = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embedimmune = new DiscordEmbedBuilder
                    {
                        Title = "Недостаточно прав!",
                        Description = $"{emojiimmune} Данный игрок имеет иммунитет к бану!",
                        Color = new DiscordColor(0xFF0000) // red
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embedimmune);
                    return;
                }

                if (offlineUserAccount.KnownIps == null)
                {
                    var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embederror = new DiscordEmbedBuilder
                    {
                        Title = "Ошибка!",
                        Description = $"{emojierror} У `{user}` не найдено ни одного IP-адресса!",
                        Color = new DiscordColor(0xFF0000) // red
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embederror);
                    return;
                }

                string lastIP = JsonConvert.DeserializeObject<List<string>>(offlineUserAccount.KnownIps).Last();

                success =
                    TShock.Bans.AddBan(lastIP,
                        "", offlineUserAccount.UUID, offlineUserAccount.Name, reason, false, ctx.User.Username,
                        banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

                CommandManager.AdvancedBan.AddBanToDB(lastIP,
                    "", offlineUserAccount.UUID, offlineUserAccount.Name, reason, ctx.User.Username,
                    banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

            }

            if (success)
            {
                var emojiban = DiscordEmoji.FromName(ctx.Client, ":b:");
                var embedban = new DiscordEmbedBuilder
                {
                    Title = "Пользователь успешно забанен!",
                    Color = new DiscordColor(0x00fd2c) // green
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                if (banLengthInSeconds == 0)
                    embedban.AddField("Description", $"{emojiban} Игрок `{user}` забанен **ПЕРМАНЕНТНО** за `{reason}`!*");
                else
                    embedban.AddField("Description", $"{emojiban} Игрок `{user}` забанен на *{time}* за `{reason}`!*");

                await ctx.RespondAsync("", embed: embedban);

                //if (!args.Silent)
                //{
                //    if (banLengthInSeconds == 0)
                //    {
                //        TSPlayer.All.SendErrorMessage("{0} was permanently banned by {1} for: {2}",
                //            targetGeneralizedName, args.Player.Account.Name, banReason);
                //    }
                //    else
                //    {
                //        TSPlayer.All.SendErrorMessage("{0} was temp banned for {1} seconds by {2} for: {3}",
                //            targetGeneralizedName, banLengthInSeconds, args.Player.Account.Name, banReason);
                //    }
                //}
            }
            else
            {
                var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embederror = new DiscordEmbedBuilder
                {
                    Title = "Системная ошибка!",
                    Description = $"{emojierror} Произошла ошибка сервера, плагина, или БД!\nПользователь не забанен!",
                    Color = new DiscordColor(0xFF0000) // red
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embederror);
            }

            return;
        }

        [Command("unban")]
        [Description("/unban <USER/IP>")]
        public async Task UnBan(CommandContext ctx, string user)
        {
            string regex = @"\w*[.]\w*[.]\w*[.]\w*";
            if (Regex.IsMatch(user, regex, RegexOptions.IgnoreCase))
            {
                string ip = user;
                Ban ban = TShock.Bans.GetBanByIp(ip);
                if (ban != null)
                {
                    if (TShock.Bans.RemoveBan(ban.IP, false))
                    {
                        var emojierror = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                        var embederror = new DiscordEmbedBuilder
                        {
                            Title = "Пользователь разбанен!",
                            Description = $"{emojierror} {ban.IP} был разбанен!\nПричина бана: {ban.Reason}.",
                            Color = new DiscordColor(0x00fd2c) // red
                                                               // there are also some pre-defined colors available
                                                               // as static members of the DiscordColor struct
                        };
                        await ctx.RespondAsync("", embed: embederror);
                        return;
                    }
                    else
                    {
                        var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                        var embederror = new DiscordEmbedBuilder
                        {
                            Title = "Системная ошибка!",
                            Description = $"{emojierror} Произошла ошибка сервера, плагина, или БД!\nПользователь не разбанен!",
                            Color = new DiscordColor(0xFF0000) // red
                                                               // there are also some pre-defined colors available
                                                               // as static members of the DiscordColor struct
                        };
                        await ctx.RespondAsync("", embed: embederror);
                    }
                }
                else 
                {
                    var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embederror = new DiscordEmbedBuilder
                    {
                        Title = "Ошибка!",
                        Description = $"{emojierror} По IP {user} не найдено ни одного бана!",
                        Color = new DiscordColor(0xFF0000) // red
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embederror);
                }
                return;
            }
            else
            {
                string plStr = user;
                Ban ban = TShock.Bans.GetBanByAccountName(plStr, true);
                if (ban != null)
                {
                    if (TShock.Bans.RemoveBan(ban.Name, true))
                    {
                        var emojierror = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                        var embederror = new DiscordEmbedBuilder
                        {
                            Title = "Пользователь разбанен!",
                            Description = $"{emojierror} `{ban.Name}` ({ban.IP}) был разбанен!\nПричина бана: {ban.Reason}.",
                            Color = new DiscordColor(0x00fd2c) // red
                                                               // there are also some pre-defined colors available
                                                               // as static members of the DiscordColor struct
                        };
                        await ctx.RespondAsync("", embed: embederror);
                        return;
                    }
                    else
                    {
                        var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                        var embederror = new DiscordEmbedBuilder
                        {
                            Title = "Системная ошибка!",
                            Description = $"{emojierror} Произошла ошибка сервера, плагина, или БД!\nПользователь не разбанен!",
                            Color = new DiscordColor(0xFF0000) // red
                                                               // there are also some pre-defined colors available
                                                               // as static members of the DiscordColor struct
                        };
                        await ctx.RespondAsync("", embed: embederror);
                    }
                }
                else
                {
                    var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embederror = new DiscordEmbedBuilder
                    {
                        Title = "Ошибка!",
                        Description = $"{emojierror} По нику {user} не найдено ни одного бана!",
                        Color = new DiscordColor(0xFF0000) // red
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embederror);
                }
                return;
            }
        }


        [Command("hardmode")]
        [Description("Включает хардмод")]
        public async Task Hardmode(CommandContext ctx)
        {
            if (ctx.Channel.Id != 565744641677197323)
                return;


            if (Main.hardMode)
            {
                string path = Path.Combine(TShock.SavePath, "config.json");
                string json = File.ReadAllText(path);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                jsonObj["DisableHardmode"] = true;
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(path, output);

                Main.hardMode = false;
                TShock.Config.DisableHardmode = true;
                TSPlayer.All.SendData(PacketTypes.WorldInfo);
                var emoji = DiscordEmoji.FromName(ctx.Client, ":skull:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Хардмод успешно выключен!",
                    Description = $"{emoji} Хардмод отключен!",
                    Color = new DiscordColor(0xFF0000) // red
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embed);
            }
            else
            {
                string path = Path.Combine(TShock.SavePath, "config.json");
                string json = File.ReadAllText(path);
                dynamic jsonObj = JsonConvert.DeserializeObject(json);
                jsonObj["DisableHardmode"] = false;
                string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                File.WriteAllText(path, output);


                TShock.Config.DisableHardmode = false;
                WorldGen.StartHardmode();
                var emoji = DiscordEmoji.FromName(ctx.Client, ":skull:");
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Хардмод успешно включен!",
                    Description = $"{emoji} Хардмод включен!",
                    Color = new DiscordColor(0x00fd2c) // green
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embed);
            }
        }
    }

    [RequireRolesAttribute("Server Staff")] // let's mark this class as a command group
    [Description("Moderator commands.")] // give it a description for help purposes
    public class ModerBotCommands
    {
        [Command("confuse")]
        [Description("Confuse player")]
        public async Task Confuse(CommandContext ctx, string user)
        {
            var players = TSPlayer.FindByNameOrID(user);
            if (players.Count == 0)
                await ctx.RespondAsync("Invalid player!");
            else if (players.Count > 1)
                await ctx.RespondAsync("Invalid player!");
            else
            {
                var ply = players[0];
                ply.Confused = !ply.Confused;
                await ctx.RespondAsync(string.Format("{0} is {1} confused.", ply.Name, ply.Confused ? "now" : "no longer"));
            }
        }

        [Command("kick")]
        [Description("/kick <player>")]
        public async Task Kick(CommandContext ctx, string user)
        {
            var players = TShockAPI.TSPlayer.FindByNameOrID(user);
            if (players.Count == 0)
            {
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Игрок не найден!",
                    Description = $"{emojiplr} По запросу `{user}` не найдено ни одного игрока!",
                    Color = new DiscordColor(0xFF0000) // red
                    // there are also some pre-defined colors available
                    // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
            else
            {
                if (!players[0].Kick("null", false, false, ctx.Member.Username, true))
                {
                    var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embedplr = new DiscordEmbedBuilder
                    {
                        Title = "Недостаточно прав!",
                        Description = $"{emojiplr} Вы не можете кикнуть `{user}`, так как он админ!",
                        Color = new DiscordColor(0xFF0000) // red
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embedplr);
                }
                else
                {
                    var emojiplr = DiscordEmoji.FromName(ctx.Client, ":ok:");
                    var embedplr = new DiscordEmbedBuilder
                    {
                        Title = "Игрок успешно кикнут!",
                        Description = $"{emojiplr} Игрок `{user}` был кикнут!",
                        Color = new DiscordColor(0x00fd2c) // green
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embedplr);
                }
            }
        }

        [Command("mute")]
        [Description("/mute <player>")]
        public async Task Mute(CommandContext ctx, string user)
        {
            var players = TShockAPI.TSPlayer.FindByNameOrID(user);
            if (players.Count == 0)
            {
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Игрок не найден!",
                    Description = $"{emojiplr} По запросу `{user}` не найдено ни одного игрока!",
                    Color = new DiscordColor(0xFF0000) // red
                    // there are also some pre-defined colors available
                    // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
            else if (players[0].HasPermission(TShockAPI.Permissions.mute))
            {
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Недостаточно прав!",
                    Description = $"{emojiplr} Вы не можете использовать мут на `{user}`, так как у него иммунитет!",
                    Color = new DiscordColor(0xFF0000) // red
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
            else if (players[0].mute)
            {
                var plr = players[0];
                plr.mute = false;
                TShockAPI.TSPlayer.All.SendInfoMessage("{0} has been unmuted by {1}.", plr.Name, ctx.Member.Username);
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":ok:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Игрок размучен!",
                    Description = $"{emojiplr} Вы успешно сняли мут с игрока `{user}`!",
                    Color = new DiscordColor(0x00fd2c) // green
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
            else
            {
                var plr = players[0];
                plr.mute = true;
                TShockAPI.TSPlayer.All.SendInfoMessage("{0} has been muted by {1}.", plr.Name, ctx.Member.Username);
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":ok:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Мут успешно выдан!",
                    Description = $"{emojiplr} Игроку `{user}` был выдан мут!",
                    Color = new DiscordColor(0x00fd2c) // green
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
        }
    }

    [Description("General commands.")] // give it a description for help purposes
    public class GeneralBotCommands
    {
        [Command("who")]
        [Description("Показывает ники всех людей на сервере")]
        public async Task Who(CommandContext ctx)
        {
            List<string> players = new List<string>();

            foreach (TSPlayer ply in TShock.Players)
            {
                if (ply != null && ply.Active)
                {
                    string boss = "";
                    //if (Plugin4PDA.Bosses.Count() > 0)
                    boss = "({0}) ".SFormat((ply.Dead) ? "Dead" : "Alive");
                    //Main.invasionType()
                    players.Add(boss + ply.Group.Prefix + ply.Name + ply.Group.Suffix);
                }
            }
            var result = String.Join("`\n`", players.ToArray());

            var emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_forward:");
            var embed = new DiscordEmbedBuilder
            {
                Title = $"***{TShock.Config.ServerName} ({Discord.Config.IP})***",
                Description = $"✅ **Сервер онлайн!**\n\n{emoji} **Online Players:**\n{TShock.Utils.GetActivePlayerCount()} | {Main.maxNetPlayers}\n \n`{result}`",
                Color = new DiscordColor(0x00fd2c) // green
                                                   // there are also some pre-defined colors available
                                                   // as static members of the DiscordColor struct
            };

            //if (Plugin4PDA.Bosses.Count() > 0)
            //{
            //    string bosses = "";
            //    foreach (Plugin4PDA.BossFight boss in Plugin4PDA.Bosses)
            //    {
            //        if (bosses == "")
            //        {
            //            int percent = (boss.Boss.life / boss.Boss.lifeMax) * 100;

            //            bosses = boss.Boss.FullName + $" ({percent}%)";
            //        }
            //        else
            //        {
            //            int percent = (boss.Boss.life / boss.Boss.lifeMax) * 100;

            //            bosses += ", " + boss.Boss.FullName + $" ({percent}%)";
            //        }
            //    }

            //    embed.AddField("Идёт сражение с боссами", $"*{bosses}*");
            //}

            await ctx.RespondAsync("", embed: embed);
        }

        [Command("info")]
        [Description("/info <player>")]
        public async Task Info(CommandContext ctx, string username)
        {
            var emojiOnline = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var emojiOffline = DiscordEmoji.FromName(ctx.Client, ":no_entry:");

            var player = TSPlayer.FindByNameOrID(username);

            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            if (!string.IsNullOrWhiteSpace(username))
            {
                #region USER
                var account = TShock.UserAccounts.GetUserAccountByName(username);
                if (account != null)
                {
                    DateTime LastSeen;
                    string Timezone = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours.ToString("+#;-#");

                    if (DateTime.TryParse(account.LastAccessed, out LastSeen))
                    {
                        LastSeen = DateTime.Parse(account.LastAccessed).ToLocalTime();

                        List<string> KnownIps = JsonConvert.DeserializeObject<List<string>>(account.KnownIps?.ToString() ?? string.Empty);
                        string ip = KnownIps?[KnownIps.Count - 1] ?? "N/A";
                        DateTime Registered = DateTime.Parse(account.Registered).ToLocalTime();
                        Ban ban = TShock.Bans.GetBanByAccountName(username, true);

                        var embed = new DiscordEmbedBuilder
                        {
                            Title = "**" + username + "**",
                            Description = (player.Count > 0) ? $"{emojiOnline} **{username} Online!**" : $"{emojiOffline} **{username} Offline!**",
                            Color = new DiscordColor(0xAC983C) // color
                                                               // there are also some pre-defined colors available
                                                               // as static members of the DiscordColor struct
                        };
                        embed.AddField("Последний вход", $"{LastSeen.ToShortDateString()} {LastSeen.ToShortTimeString()} UTC{Timezone}.");
                        embed.AddField("Группа", account.Group);
                        embed.AddField("IP", (player.Count > 1) ? player[0].IP : ip);
                        embed.AddField("Дата регистрации", $"{Registered.ToShortDateString()} {Registered.ToShortTimeString()} UTC{Timezone}.");
                        embed.AddField("Текущий бан", (ban != null) ? $"By *{ban.BanningUser}*" +
                            $"\nDate: *{ban.BanDateTime}*" +
                            $"\nExpiration: *{ban.ExpirationDateTime}*\n " +
                            $"\n**Reason:** `{ban.Reason}`"
                            : "Бана нет!");

                        List<Ban> banlist = CommandManager.AdvancedBan.GetBans("", account.Name);
                        string bans = null;
                        int i = 1;

                        if (banlist.Count > 0)
                        {
                            foreach (Ban item in banlist)
                            {
                                bans += $"{i}.\n" +
                                $"By *{item.BanningUser}*" +
                                $"\nDate: *{item.Date}*" +
                                $"\nExpiration: *{item.Expiration}*" +
                                $"\n**Reason:** `{item.Reason}`\n \n";

                                i++;
                            }
                            embed.AddField("Все баны", bans);
                        }

                        await ctx.RespondAsync("", embed: embed);
                        return;
                    }
                }
                if (r.IsMatch(username))
                {
                    #region IP
                    Ban ban = TShock.Bans.GetBanByIp(username);
                    List<Ban> bans = CommandManager.AdvancedBan.GetBans(username);
                    string accounts = null;

                    List<string> accountlist = AntiCheat.Utils.GetUserAccountByIP(username);

                    if (accountlist.Count > 0)
                    {
                        foreach (string accountPly in accountlist)
                        {
                            accounts += "`" + accountPly + "` ";
                        }
                    }

                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "**" + username + "**",
                        Description = (accountlist.Count > 0) ? $"{emojiOnline} **Известные аккаунты:** {accounts}" : $"{emojiOffline} **Аккаунты не найдены!**",
                        Color = new DiscordColor(0xAC983C) // color
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };

                    string expBan = (ban.Expiration != "" && ban.Expiration != null) ? $"*{ban.Expiration}*\n" : "**ПЕРМАНЕНТ**\n";

                    embed.AddField("Текущий бан", (ban != null) ? $"**Username: {ban.AccountName}**" +
                                $"\nBy *{ban.BanningUser}*" +
                                $"\nDate: *{ban.BanDateTime}*" +
                                $"\nExpiration:" +
                                expBan +
                                $"\n**Reason:** `{ban.Reason}`"
                                : "Бана нет!");

                    string banlist = null;
                    int i = 1;

                    if (bans.Count > 0)
                    {
                        foreach (Ban item in bans)
                        {
                            string exp = (item.Expiration != "" && item.Expiration != null) ? $"*{item.Expiration}*\n" : "**ПЕРМАНЕНТ**\n";

                            banlist += $"{i}. **{item.AccountName}**" +
                            $"\nBy *{item.BanningUser}*" +
                            $"\nDate: *{item.Date}*" +
                            $"\nExpiration: " +
                            exp +
                            $"\n**Reason:** `{item.Reason}`\n \n";

                            i++;
                        }
                        embed.AddField("Все баны", banlist);
                    }

                    await ctx.RespondAsync("", embed: embed);
                    return;
                    #endregion
                }
                else
                {
                    var emoji = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Игрок не найден!",
                        Description = $"{emoji} По запросу `{username}` не найдено ни одного игрока!",
                        Color = new DiscordColor(0xFF0000) // red
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embed);
                    return;
                }
                #endregion
            }
            else
            {
                #region ERROR
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Неправильный синтаксис!",
                    Description = $"{emojiplr} /info <account/username/ip>",
                    Color = new DiscordColor(0xFF0000) // red
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embedplr);
                #endregion
            }
        }

        [Command("inv")]
        [Description("/inv <player>")]
        public async Task Inv(CommandContext ctx, string username)
        {
            if (ctx.Channel.Id == 628805818929905664 || ctx.Channel.Id == 695528646907133983 || ctx.Channel.Id == 565583752126791687)
            {
                await ctx.RespondAsync("Please, go to the #botlogs!");
                return;
            }

            var player = TSPlayer.FindByNameOrID(username);
            if (!string.IsNullOrWhiteSpace(username))
            {
                if (player.Count > 0)
                {
                    Item trashItem = player[0].TPlayer.trashItem;

                    string msg = "Trash: " +
                        trashItem.Name??"Empty" +
                        "\n\nPiggy:";


                    Chest piggyBank = player[0].TPlayer.bank;
                    Chest safeBank = player[0].TPlayer.bank2;
                    foreach (Item pig in piggyBank.item)
                    {
                        if (pig.Name == null || pig.Name == "")
                            continue;
                        msg += "\n`" + pig.Name + "`" + " - " + pig.stack;
                    }

                    msg += "\n \n" + "Safe:";
                    foreach (Item safe in safeBank.item)
                    {
                        if (safe.Name == null || safe.Name == "")
                            continue;
                        msg += "\n`" + safe.Name + "`" + " - " + safe.stack;
                    }

                    var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = $"{emoji} **" + username + "**",
                        Description = msg,
                        Color = new DiscordColor(0xAC983C) // color
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embed);
                    return;
                }
                else
                {
                    var emoji = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Игрок не найден!",
                        Description = $"{emoji} По запросу `{username}` не найдено ни одного игрока!",
                        Color = new DiscordColor(0xFF0000) // red
                                                           // there are also some pre-defined colors available
                                                           // as static members of the DiscordColor struct
                    };
                    await ctx.RespondAsync("", embed: embed);
                    return;
                }
            }
            else
            {
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Неправильный синтаксис!",
                    Description = $"{emojiplr} /inv <account/username>",
                    Color = new DiscordColor(0xFF0000) // red
                                                       // there are also some pre-defined colors available
                                                       // as static members of the DiscordColor struct
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
        }
    }

    //[Description("Clan commands.")] // give it a description for help purposes
    //public class ClanBotCommands
    //{
    //    [RequireRolesAttribute("VIP/PREM")]
    //    [Command("confirm")]
    //    [Description("/confirm <name>")]
    //    public async Task LinkAccount(CommandContext ctx, string account)
    //    {
    //        UserAccount acc = TShock.UserAccounts.GetUserAccountByName(account);

    //        if (acc == null || acc.Name == null)
    //        {
    //            var emoji = DiscordEmoji.FromName(ctx.Client, ":warning:");
    //            var embed = new DiscordEmbedBuilder
    //            {
    //                Title = "Игрок не найден!",
    //                Description = $"{emoji} По запросу `{account}` не найдено ни одного игрока!",
    //                Color = new DiscordColor(0xFF0000) // red
    //                                                   // there are also some pre-defined colors available
    //                                                   // as static members of the DiscordColor struct
    //            };
    //            await ctx.RespondAsync("", embed: embed);
    //            return;
    //        }
    //        else
    //        {
    //            TShockAPI.Group group = TShock.Groups.GetGroupByName(acc.Group);

    //            if (group.HasPermission("tshock.prem"))
    //            {
    //                if ((DateTime.Now - DateTime.Parse(acc.Registered).ToLocalTime()).TotalDays >= 30)
    //                {
    //                    ClanManager Clan = new ClanManager();

    //                    if (!Clan.AddPrem(ctx.User.Id, acc.ID, acc.Name, true))
    //                    {
    //                        var emojie = DiscordEmoji.FromName(ctx.Client, ":warning:");
    //                        var embede = new DiscordEmbedBuilder
    //                        {
    //                            Title = "Данный игрок уже зарегистрирован, или ожидает подтвеждение!",
    //                            Description = $"{emojie} Найден аккаунт `{account}`, но он уже есть в базе.",
    //                            Color = new DiscordColor(0xFF0000) // red
    //                                                               // there are also some pre-defined colors available
    //                                                               // as static members of the DiscordColor struct
    //                        };
    //                        await ctx.RespondAsync("", embed: embede);
    //                        return;
    //                    }
    //                    else
    //                    {
    //                        var emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_forward:");
    //                        var embed = new DiscordEmbedBuilder
    //                        {
    //                            Title = $"***Успешно!***",
    //                            Description = $"✅ Ваш аккаунт \"{account}\" ожидает проверки! Зайдите на сервер под этим ником и напишите \"/confirm\".",
    //                            Color = new DiscordColor(0x00fd2c) // green
    //                                                               // there are also some pre-defined colors available
    //                                                               // as static members of the DiscordColor struct
    //                        };
    //                        await ctx.RespondAsync("", embed: embed);
    //                    }

    //                    try
    //                    {
    //                        ClanManager.AwaitConfirm.Add(new ClanManager.Confirmation(acc, ctx.User, 0));
    //                    }
    //                    catch (NullReferenceException ex)
    //                    {
    //                        Console.WriteLine("ADOMOXZ<");
    //                    }
                        
    //                    //foreach (UserAccount a in Clan.AwaitConfirm)
    //                        //Console.WriteLine(a.Name);

    //                    return;
    //                }
    //                else
    //                {
    //                    double days = 30 - (DateTime.Now - DateTime.Parse(acc.Registered).ToLocalTime()).TotalDays;

    //                    var emoji = DiscordEmoji.FromName(ctx.Client, ":warning:");
    //                    var embed = new DiscordEmbedBuilder
    //                    {
    //                        Title = "Данный игрок не пробыл достаточное время на сервере!",
    //                        Description = $"{emoji} Найден аккаунт `{account}`, но он младше 30 дней! Осталось {days}.",
    //                        Color = new DiscordColor(0xFF0000) // red
    //                                                           // there are also some pre-defined colors available
    //                                                           // as static members of the DiscordColor struct
    //                    };
    //                    await ctx.RespondAsync("", embed: embed);
    //                    return;
    //                }
    //            }
    //            else
    //            {
    //                var emoji = DiscordEmoji.FromName(ctx.Client, ":warning:");
    //                var embed = new DiscordEmbedBuilder
    //                {
    //                    Title = "Данный игрок не имеет премиума!",
    //                    Description = $"{emoji} Найден аккаунт `{account}`, но он в группе **{group.Name}**, а не [PREM]!",
    //                    Color = new DiscordColor(0xFF0000) // red
    //                                                       // there are also some pre-defined colors available
    //                                                       // as static members of the DiscordColor struct
    //                };
    //                await ctx.RespondAsync("", embed: embed);
    //                return;
    //            }
    //        }
    //    }

    //    [RequireRolesAttribute("Confirmed Prem")]
    //    [Command("clan")]
    //    [Description("/clan <add|del|edit|help> <name>")]
    //    public async Task ClanMaster(CommandContext ctx, string option, string name)
    //    {
    //        var interactivity = ctx.Client.GetInteractivityModule();

    //        ClanManager Clans = new ClanManager();
    //        option = option.ToLower();

    //        switch (option)
    //        {
    //            case "add":
    //                {
    //                    if (name.Length > 16 || name.Length < 4)
    //                    {
    //                        await ctx.RespondAsync("Error! Name of clan should be less than 16 chars and more than 4!");
    //                        return;
    //                    }

    //                    ClanManager.PremAccount user = ClanManager.PremAccounts.Where(u => u.Discord == ctx.User).First();

    //                    if (ClanManager.ClanList.Where(u => u.Name == name || u.Owner == user.Discord).Count() > 0)
    //                    {
    //                        await ctx.RespondAsync("Error! Clan with this name already exist, or you are owner of another clan!");
    //                        return;
    //                    }

    //                    var embed = new DiscordEmbedBuilder();
    //                    DiscordMessage msg;
    //                    embed.Color = new DiscordColor(0xFF00DE);

    //                    var emojiEn = DiscordEmoji.FromName(ctx.Client, ":flag_gb:");
    //                    var emojiRu = DiscordEmoji.FromName(ctx.Client, ":flag_ru:");

    //                    #region LANG
    //                    if (user.Lang == 0) 
    //                    { 
    //                        embed.Title = "Choose your language!";
    //                        embed.Description = "This choice does not affect the clan!\nЭтот выбор не повлияет на клан!";
    //                        embed.AddField("Languages", "`[1]` English\n" +
    //                            "`[2]` Русский");

    //                        msg = ctx.RespondAsync(embed: embed).Result;

    //                        var lang = await interactivity.WaitForMessageAsync(xm => ((xm.Content == "1" || xm.Content == "2") && xm.Author == ctx.User), TimeSpan.FromSeconds(60));

    //                        user.Lang = byte.Parse(lang.Message.Content);

    //                        Clans.PremChangeLang(user);
    //                        await msg.DeleteAsync();
    //                        await lang.Message.DeleteAsync();
    //                        await Task.Delay(1000);
    //                    }
    //                    #endregion

    //                    DiscordEmoji ok = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
    //                    DiscordEmoji cancel = DiscordEmoji.FromName(ctx.Client, ":x:");

    //                    #region Welcome
    //                    embed.ClearFields();
    //                    embed.Title = (user.Lang == 2) ? "Мастер создания клана" : "--";
    //                    embed.Description = (user.Lang == 2) ? "Добро пожаловать в мастер создания клана.\n" +
    //                        "Отвечайте реакциями, когда надо - отвечайте прямо в чат!" :
    //                        "Welcome! " +
    //                        "Answer quations with reactions, sometimes write answers into the chat!";
    //                    embed.AddField("Answers", $"`[ok]`    OK\n \n \n`[cancel]`    Cancel");

    //                    msg = ctx.RespondAsync(embed: embed).Result;

    //                    var welcome = await interactivity.WaitForMessageAsync(xm => ((xm.Content == "ok" || xm.Content == "cancel") && xm.Author == ctx.User), TimeSpan.FromSeconds(60));
    //                    if (welcome.Message.Content == "cancel")
    //                    {
    //                        await msg.DeleteAsync();
    //                        await welcome.Message.DeleteAsync();
    //                        return;
    //                    }

    //                    await Task.Delay(1000);
    //                    #endregion

    //                    #region Clan-LANG
    //                    List<DiscordEmoji> listEmoji = new List<DiscordEmoji>();
    //                    var emojiFlag = DiscordEmoji.FromName(ctx.Client, ":flag_white:");

    //                    embed.ClearFields();
    //                    embed.Title = (user.Lang == 2) ? "Выберите язык клана" : "Choose clan language";
    //                    embed.Description = (user.Lang == 2) ? "Если вы хотите, чтобы к вашему клану присоединялись **только** люди, говорящие на данном языке." : "If you want only people speaking this language to join your clan.";
    //                    embed.AddField("Languages", $"`[1]`    ***English***" +
    //                        $"\n \n`[2]`    ***Русский***" +
    //                        $"\n \n`[3]`    ***Любой***" +
    //                        $"\n \n \n`[cancel]`    Cancel");

    //                    await msg.DeleteAsync();
    //                    await welcome.Message.DeleteAsync();
    //                    msg = ctx.RespondAsync(embed: embed).Result;

    //                    var clanLang = await interactivity.WaitForMessageAsync(xm => ((xm.Content == "1" || xm.Content == "2" || xm.Content == "3" || xm.Content == "cancel") && xm.Author == ctx.User), TimeSpan.FromSeconds(60));
    //                    if (clanLang.Message.Content == "cancel")
    //                    {
    //                        await msg.DeleteAsync();
    //                        await clanLang.Message.DeleteAsync();
    //                        return;
    //                    }
    //                    embed.ClearFields();

    //                    await Task.Delay(1000);
    //                    #endregion

    //                    #region Suffix
    //                    embed.Title = (user.Lang == 2) ? "Выберите суффикс клана" : "Choose clan suffix";
    //                    embed.Description = (user.Lang == 2) ? "Суффикс стоит после ника игрока. " +
    //                        "Ниже представлен автоматически сгенерированный суффикс из названия клана." +
    //                        "Если он вам не нравится - вы можете ввести свой (без квадратных скобок). Длинной он должен быть не более 4 символов."
    //                        : "";

    //                    var pattern = @"((?<=^|\s)(\w{1})|([A-ZА-Я]))";
    //                    string suffix = string.Join(string.Empty, Regex.Matches(name, pattern).OfType<Match>().Select(x => x.Value.ToUpper()));

    //                    if (name.Length == 4)
    //                    {
    //                        suffix = name.ToUpper();
    //                    }
    //                    else if (suffix.Length > 4)
    //                        suffix = suffix.Substring(0, 4);

    //                    IEnumerable<ClanManager.Clan> prList = from clans in ClanManager.ClanList
    //                                                             where clans.Suffix == suffix
    //                                                             select clans;

    //                    if (prList.Count() > 0)
    //                        suffix = "Суффикс не был сгенерирован! Введите свой.";

    //                    embed.AddField("Suffix", $"`{suffix}`" +
    //                        $"\n \n \n`[cancel]`    `cancel`");

    //                    await msg.DeleteAsync();
    //                    await clanLang.Message.DeleteAsync();
    //                    msg = ctx.RespondAsync(embed: embed).Result;

    //                    await Task.Delay(1000);

    //                    var suffixName = await interactivity.WaitForMessageAsync(xm => (xm.Content.Length <= 4 && xm.Author == ctx.User), TimeSpan.FromSeconds(60));

    //                    if (suffixName.Message.Content != suffix)
    //                    {
    //                        IEnumerable<ClanManager.Clan> suffixList = from clans in ClanManager.ClanList
    //                                                                   where clans.Suffix == suffixName.Message.Content
    //                                                                   select clans;
    //                        if (suffixList.Count() > 0)
    //                        {
    //                            await msg.DeleteAsync();
    //                            await suffixName.Message.DeleteAsync();
    //                            await ctx.RespondAsync("Данный суффикс занят!", embed: null);
    //                            return;
    //                        }
    //                        else
    //                            suffix = suffixName.Message.Content;
    //                    }

    //                    embed.ClearFields();
    //                    await msg.DeleteAllReactionsAsync();
    //                    await Task.Delay(1000);
    //                    #endregion

    //                    #region JoinType

    //                    embed.Title = (user.Lang == 2) ? "Выберете тип принятия людей в клан" : "--";
    //                    embed.Description = (user.Lang == 2) ? "Принимать людей вы можете двумя способами: самостоятельно, с помощью команды, или открыть клан для всех. " +
    //                        "Во втором случае любой (или подходящий под требования) игрок сможет присоединиться." : "--";

    //                    string open = (user.Lang == 2) ? "Открытый доступ" : "Open access";
    //                    string close = (user.Lang == 2) ? "Зыкрытый доступ" : "Close access";

    //                    embed.AddField("Types", $"`[1]`    **{open}**" +
    //                        $"\n \n`[2]`    **{close}**" +
    //                        $"\n \n \n`[cancel]`    Сancel");

    //                    await msg.DeleteAsync();
    //                    await suffixName.Message.DeleteAsync();
    //                    Console.WriteLine(suffixName.Message.Content);
    //                    msg = ctx.RespondAsync(embed: embed).Result;

    //                    var joinType = await interactivity.WaitForMessageAsync(xm => ((xm.Content == "1" || xm.Content == "2" || xm.Content == "cancel") && xm.Author == ctx.User), TimeSpan.FromSeconds(60));

    //                    if (joinType.Message.Content == "cancel")
    //                    {
    //                        await msg.DeleteAsync();
    //                        await joinType.Message.DeleteAsync();
    //                        return;
    //                    }

    //                    embed.ClearFields();
    //                    await Task.Delay(1000);
    //                    #endregion

    //                    #region Description
    //                    embed.Title = (user.Lang == 2) ? "Описание клана" : "Clan description";
    //                    embed.Description = (user.Lang == 2) ? "Напишите хорошее описание вашему клану, не более 200 символов!" : "--";

    //                    await msg.DeleteAsync();
    //                    await joinType.Message.DeleteAsync();
    //                    Console.WriteLine(suffixName.Message.Content);
    //                    msg = ctx.RespondAsync(embed: embed).Result;

    //                    var description = await interactivity.WaitForMessageAsync(xm => (xm.Content.Length <= 200 && xm.Author == ctx.User), TimeSpan.FromSeconds(60));

    //                    embed.ClearFields();
    //                    #endregion

    //                    #region Info
    //                    embed.Title = (user.Lang == 2) ? $"{name}" : $"{name}";
    //                    embed.Description = (user.Lang == 2) ? "На этом создание клана окончено! " +
    //                        "Если вы допустили ошибку, не переживайте - напишите */clan edit <name>*, там вы сможете изменить всё, что угодно (КРОМЕ НАЗВАНИЯ КЛАНА)! " +
    //                        "Учтите, у вас есть неделя, чтобы набрать людей! Не менее 3! " +
    //                        "По крайней мере 1 из них должен быть вашим заместителем.\n" +
    //                        "У каждого члена сервера есть уровень, который будет подниматься в зависимости от набранного опыта. " +
    //                        "Опыт - сумма опыта из татсумаки (активность в дискорде) + активность на сервере. " +
    //                        "Кланы также имеют опыт - сумма опыта всех участников клана. " +
    //                        "В зависимости от уровня клана - будут пояивлятся новые плюшки (увеличенный суффикс), свои каналы и тд.\n \n" +
    //                        "Сейчас были автоматически созданы и выданы вам права, роли и канал клана!" : "--";

    //                    await msg.DeleteAsync();
    //                    await description.Message.DeleteAsync();
    //                    await ctx.RespondAsync(embed: embed);
    //                    #endregion

    //                    if (Clans.AddClan(user.Discord.Id, name, int.Parse(clanLang.Message.Content), suffix, int.Parse(joinType.Message.Content), description.Message.Content))
    //                    {
    //                        ClanManager.ClanList.Add(new ClanManager.Clan(name, user.Discord, new List<UserAccount>(), new List<ClanManager.Roles>(), suffix, int.Parse(joinType.Message.Content), description.Message.Content));
    //                    }
    //                    else
    //                    {
    //                        var mort = await ctx.Guild.GetMemberAsync(406114228727185408);
    //                        await ctx.RespondAsync("Error! Please, try again, or contact whith " + mort.Mention);
    //                    }

    //                    return;
    //                }
    //        }
    //    }
    //}
}
