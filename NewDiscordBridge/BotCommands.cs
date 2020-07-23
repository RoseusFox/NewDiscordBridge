using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Newtonsoft.Json;

using Terraria;

using TShockAPI;
using TShockAPI.DB;

namespace Terraria4PDA.DiscordBridge
{
    public class BotCommands
    {
        [Command("who")]
        [Description("Displays a list of players")]
        public async Task Who(CommandContext ctx)
        {
            List<string> players = new List<string>();

            foreach (TSPlayer ply in TShock.Players)
            {
                if (ply != null && ply.Active)
                {
                    string status = "({0}) ".SFormat((ply.Dead) ? "Dead" : "Alive");
                    players.Add(status + ply.Group.Prefix + ply.Name + ply.Group.Suffix);
                }
            }
            var result = string.Join("`\n`", players.ToArray());

            var emoji = DiscordEmoji.FromName(ctx.Client, ":arrow_forward:");
            var embed = new DiscordEmbedBuilder
            {
                Title = $"***{((string.IsNullOrWhiteSpace(TShock.Config.ServerName)) ? Main.worldName : TShock.Config.ServerName)}***",
                Description = $"{emoji} **Online Players:**\n{TShock.Utils.GetActivePlayerCount()} | {Main.maxNetPlayers}\n \n`{result}`",
                Color = new DiscordColor(0x00fd2c) // green
                                                   // there are also some pre-defined colors available
                                                   // as static members of the DiscordColor struct
            };

            await ctx.RespondAsync("", embed: embed);
        }

        [Command("info")]
        [Description("Shows information about the player's account or IP")]
        public async Task Info(CommandContext ctx, string username)
        {
            var emojiOnline = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var emojiOffline = DiscordEmoji.FromName(ctx.Client, ":no_entry:");

            var player = TSPlayer.FindByNameOrID(username);

            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);

            if (!string.IsNullOrWhiteSpace(username))
            {
                var account = TShock.UserAccounts.GetUserAccountByName(username);

                if (r.IsMatch(username))
                {
                    #region IP
                    Ban ban = TShock.Bans.GetBanByIp(username);
                    string accounts = null;

                    List<string> accountlist = Utils.GetUserAccountByIP(username);

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
                        Description = (accountlist.Count > 0) ? $"{emojiOnline} **Accounts:** {accounts}" : $"{emojiOffline} **No accounts found!**",
                        Color = new DiscordColor(0xAC983C)
                    };

                    string expBan = (ban.Expiration != "" && ban.Expiration != null) ? $"*{ban.Expiration}*\n" : "**PERMANENT**\n";

                    embed.AddField("Current ban", (ban != null) ? $"**Username: {ban.AccountName}**" +
                                $"\nBy *{ban.BanningUser}*" +
                                $"\nDate: *{ban.BanDateTime}*" +
                                $"\nExpiration:" +
                                expBan +
                                $"\n**Reason:** `{ban.Reason}`"
                                : "No ban!");

                    //string banlist = null;
                    //int i = 1;

                    //if (bans.Count > 0)
                    //{
                    //    foreach (Ban item in bans)
                    //    {
                    //        string exp = (item.Expiration != "" && item.Expiration != null) ? $"*{item.Expiration}*\n" : "**ПЕРМАНЕНТ**\n";

                    //        banlist += $"{i}. **{item.AccountName}**" +
                    //        $"\nBy *{item.BanningUser}*" +
                    //        $"\nDate: *{item.Date}*" +
                    //        $"\nExpiration: " +
                    //        exp +
                    //        $"\n**Reason:** `{item.Reason}`\n \n";

                    //        i++;
                    //    }
                    //    embed.AddField("Все баны", banlist);
                    //}

                    await ctx.RespondAsync("", embed: embed);
                    return;
                    #endregion
                }

                if (account != null)
                {
                    #region USER
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
                            Color = new DiscordColor(0xAC983C) 
                        };
                        embed.AddField("Last login occured", $"{LastSeen.ToShortDateString()} {LastSeen.ToShortTimeString()} UTC{Timezone}.");
                        embed.AddField("Group", account.Group);
                        embed.AddField("IP", (player.Count > 1) ? player[0].IP : ip);
                        embed.AddField("Register date", $"{Registered.ToShortDateString()} {Registered.ToShortTimeString()} UTC{Timezone}.");
                        embed.AddField("Current ban", (ban != null) ? $"By *{ban.BanningUser}*" +
                            $"\nDate: *{ban.BanDateTime}*" +
                            $"\nExpiration: *{ban.ExpirationDateTime}*\n " +
                            $"\n**Reason:** `{ban.Reason}`"
                            : "No ban!");

                        //List<Ban> banlist = CommandManager.AdvancedBan.GetBans("", account.Name);
                        //string bans = null;
                        //int i = 1;

                        //if (banlist.Count > 0)
                        //{
                        //    foreach (Ban item in banlist)
                        //    {
                        //        bans += $"{i}.\n" +
                        //        $"By *{item.BanningUser}*" +
                        //        $"\nDate: *{item.Date}*" +
                        //        $"\nExpiration: *{item.Expiration}*" +
                        //        $"\n**Reason:** `{item.Reason}`\n \n";

                        //        i++;
                        //    }
                        //    embed.AddField("All bans", bans);
                        //}

                        await ctx.RespondAsync("", embed: embed);
                        return;
                    }
                    #endregion
                }
                else
                {
                    var emoji = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embed = new DiscordEmbedBuilder
                    {
                        Title = "Account or IP not found!",
                        Description = $"{emoji} `{username}` not found!",
                        Color = new DiscordColor(0xFF0000)
                    };
                    await ctx.RespondAsync("", embed: embed);
                    return;
                }
            }
            else
            {
                #region ERROR
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Invalid syntax!",
                    Description = $"{emojiplr} /info <account/username/ip>",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await ctx.RespondAsync("", embed: embedplr);
                #endregion
            }
        }

        //[Command("confuse")]
        //[Description("Confuse player")]
        //public async Task Confuse(CommandContext ctx, string user)
        //{
        //    var players = TSPlayer.FindByNameOrID(user);
        //    if (players.Count == 0)
        //        await ctx.RespondAsync("Invalid player!");
        //    else if (players.Count > 1)
        //        await ctx.RespondAsync("Invalid player!");
        //    else
        //    {
        //        var ply = players[0];
        //        ply.Confused = !ply.Confused;
        //        await ctx.RespondAsync(string.Format("{0} is {1} confused.", ply.Name, ply.Confused ? "now" : "no longer"));
        //    }
        //}

        [RequirePermissions(DSharpPlus.Permissions.MuteMembers)]
        [Command("mute")]
        [Description("Mute player")]
        public async Task Mute(CommandContext ctx, string player)
        {
            var players = TSPlayer.FindByNameOrID(player);
            if (players.Count == 0)
            {
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Player not found!",
                    Description = $"{emojiplr} `{player}` not found!",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
            else if (players[0].HasPermission(Permissions.mute))
            {
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Permission denied!",
                    Description = $"{emojiplr} `{player}` is immune to mute!",
                    Color = new DiscordColor(0xFF0000) // red
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
                    Title = "Player has been unmuted!",
                    Description = $"{emojiplr} `{player}` has been unmuted!",
                    Color = new DiscordColor(0x00fd2c) // green
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
                    Title = "Player has been muted!",
                    Description = $"{emojiplr} Player `{player}` has been muted!",
                    Color = new DiscordColor(0x00fd2c) // green
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
        }

        [RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        [Command("kick")]
        [Description("/kick <player>")]
        public async Task Kick(CommandContext ctx, string player, string reason = null)
        {
            var players = TSPlayer.FindByNameOrID(player);
            if (players.Count == 0)
            {
                var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embedplr = new DiscordEmbedBuilder
                {
                    Title = "Player not found!",
                    Description = $"{emojiplr} `{player}` not found!",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await ctx.RespondAsync("", embed: embedplr);
            }
            else
            {
                if (!players[0].Kick(reason??"null", false, false, ctx.Member.Username, true))
                {
                    var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embedplr = new DiscordEmbedBuilder
                    {
                        Title = "Permission denied!",
                        Description = $"{emojiplr} `{player}`is immune to kick!",
                        Color = new DiscordColor(0xFF0000) // red
                    };
                    await ctx.RespondAsync("", embed: embedplr);
                }
                else
                {
                    var emojiplr = DiscordEmoji.FromName(ctx.Client, ":ok:");
                    var embedplr = new DiscordEmbedBuilder
                    {
                        Title = "Player was kicked!",
                        Description = $"{emojiplr} Player `{player}` was kicked{((!string.IsNullOrWhiteSpace(reason)) ? $" for `{reason}`" : "")}!",
                        Color = new DiscordColor(0x00fd2c) // green
                    };
                    await ctx.RespondAsync("", embed: embedplr);
                }
            }
        }

        [RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        [Command("ban")]
        [Description("Ban player or IP")]
        public async Task Ban(CommandContext ctx, string user, string time, string reason = "no reason")
        {
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
                    Title = "Invalid time format",
                    Description = $"{emojitime} Example: *0 (permanent), 10d 5h 3m 2s*",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await ctx.RespondAsync("", embed: embedtime);
                return;
            }

            if (players.Count == 1)
            {
                TSPlayer target = players[0];
                if (target.HasPermission(Permissions.immunetoban))
                {
                    var emojiimmune = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embedimmune = new DiscordEmbedBuilder
                    {
                        Title = "Permissions denied!",
                        Description = $"{emojiimmune} Player is immune to ban!",
                        Color = new DiscordColor(0xFF0000) // red
                    };
                    await ctx.RespondAsync("", embed: embedimmune);
                    return;
                }

                targetGeneralizedName = target.Name;
                success = TShock.Bans.AddBan(target.IP, target.Name, target.UUID, target.Account?.Name ?? "", reason, false, ctx.User.Username + "#" + ctx.User.Discriminator,
                    banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

                //Terraria4PDA.CommandManager.AdvancedBan.AddBanToDB(target.IP, target.Name, target.UUID, target.Account?.Name ?? "", reason, ctx.User.Username + "#" + ctx.User.Discriminator,
                //    banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

                if (success)
                {
                    if (banLengthInSeconds == 0)
                    {
                        target.Disconnect(string.Format("Permanently banned for {0}", reason));
                    }
                    else
                    {
                        target.Disconnect(string.Format("Banned for {0} seconds for {1}", banLengthInSeconds, reason));
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

                    //CommandManager.AdvancedBan.AddBanToDB(user, "", "", "", reason,
                    //    ctx.User.Username, banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));


                    if (success && offlineUserAccount != null)
                    {
                        var emojiip = DiscordEmoji.FromName(ctx.Client, ":b:");
                        var embedip = new DiscordEmbedBuilder
                        {
                            Title = $"{emojiip} IP banned successfully!",
                            Description = $"**IP:** `{user}`\n**Time:** `{time}`\n**Reason:** `{reason}`",
                            Color = new DiscordColor(0x00fd2c) // green
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
                            Title = "Invalid player/IP!",
                            Description = $"{emojierror} `{user}` not found!!",
                            Color = new DiscordColor(0xFF0000) // red
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
                        Title = "Permissions denied!",
                        Description = $"{emojiimmune} Player is immune to ban!",
                        Color = new DiscordColor(0xFF0000) // red
                    };
                    await ctx.RespondAsync("", embed: embedimmune);
                    return;
                }

                if (offlineUserAccount.KnownIps == null)
                {
                    var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embederror = new DiscordEmbedBuilder
                    {
                        Title = "Error!",
                        Description = $"{emojierror} `{user}` have no valid IP to ban.",
                        Color = new DiscordColor(0xFF0000) // red
                    };
                    await ctx.RespondAsync("", embed: embederror);
                    return;
                }

                string lastIP = JsonConvert.DeserializeObject<List<string>>(offlineUserAccount.KnownIps).Last();

                success =
                    TShock.Bans.AddBan(lastIP,
                        "", offlineUserAccount.UUID, offlineUserAccount.Name, reason, false, ctx.User.Username,
                        banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

                //CommandManager.AdvancedBan.AddBanToDB(lastIP,
                //    "", offlineUserAccount.UUID, offlineUserAccount.Name, reason, ctx.User.Username,
                //    banLengthInSeconds == 0 ? "" : DateTime.UtcNow.AddSeconds(banLengthInSeconds).ToString("s"));

            }

            if (success)
            {
                var emojiban = DiscordEmoji.FromName(ctx.Client, ":b:");
                var embedban = new DiscordEmbedBuilder
                {
                    Title = "Player successfully banned!",
                    Color = new DiscordColor(0x00fd2c) // green
                };
                if (banLengthInSeconds == 0)
                    embedban.AddField("Description", $"{emojiban} Player `{user}` was successfully banned **PERMANENTLY** for `{reason}`!*");
                else
                    embedban.AddField("Description", $"{emojiban} Player `{user}` was successfully banned for *{time}* for `{reason}`!*");

                await ctx.RespondAsync("", embed: embedban);
            }
            else
            {
                var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                var embederror = new DiscordEmbedBuilder
                {
                    Title = "System error!",
                    Description = $"{emojierror} Player was NOT banned due to a database error or other system problem.",
                    Color = new DiscordColor(0xFF0000) // red
                };
                await ctx.RespondAsync("", embed: embederror);
            }

            return;
        }

        [RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        [Command("unban")]
        [Description("Unban player or IP")]
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
                            Title = "IP unbanned!",
                            Description = $"{emojierror} {ban.IP} was unbanned!\nBan reason: {ban.Reason}.",
                            Color = new DiscordColor(0x00fd2c) // red
                        };
                        await ctx.RespondAsync("", embed: embederror);
                        return;
                    }
                    else
                    {
                        var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                        var embederror = new DiscordEmbedBuilder
                        {
                            Title = "System error!",
                            Description = $"{emojierror} IP was NOT unbanned due to a database error or other system problem.",
                            Color = new DiscordColor(0xFF0000) // red
                        };
                        await ctx.RespondAsync("", embed: embederror);
                    }
                }
                else
                {
                    var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embederror = new DiscordEmbedBuilder
                    {
                        Title = "Not found",
                        Description = $"{emojierror} Ban with this IP not found!",
                        Color = new DiscordColor(0xFF0000) // red
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
                            Title = "Player was unbanned!",
                            Description = $"{emojierror} `{ban.Name}` ({ban.IP}) was unbanned!\nBan reason: {ban.Reason}.",
                            Color = new DiscordColor(0x00fd2c) // red
                        };
                        await ctx.RespondAsync("", embed: embederror);
                        return;
                    }
                    else
                    {
                        var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                        var embederror = new DiscordEmbedBuilder
                        {
                            Title = "System error!",
                            Description = $"{emojierror} Player was NOT unbanned due to a database error or other system problem.",
                            Color = new DiscordColor(0xFF0000) // red
                        };
                        await ctx.RespondAsync("", embed: embederror);
                    }
                }
                else
                {
                    var emojierror = DiscordEmoji.FromName(ctx.Client, ":warning:");
                    var embederror = new DiscordEmbedBuilder
                    {
                        Title = "Not found!",
                        Description = $"{emojierror} {user} has no bans!",
                        Color = new DiscordColor(0xFF0000) // red
                    };
                    await ctx.RespondAsync("", embed: embederror);
                }
                return;
            }
        }

        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        [Command("off")]
        [Description("Shuts down the server while saving.")]
        public async Task Off(CommandContext ctx)
        {
            await ctx.RespondAsync("Shutting down...");

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

            TShock.Utils.StopServer(true, "Server shutting down!");
        }

        //[Command("inv")]
        //[Description("/inv <player>")]
        //public async Task Inv(CommandContext ctx, string username)
        //{
        //    if (ctx.Channel.Id == 628805818929905664 || ctx.Channel.Id == 695528646907133983 || ctx.Channel.Id == 565583752126791687)
        //    {
        //        await ctx.RespondAsync("Please, go to the #botlogs!");
        //        return;
        //    }

        //    var player = TSPlayer.FindByNameOrID(username);
        //    if (!string.IsNullOrWhiteSpace(username))
        //    {
        //        if (player.Count > 0)
        //        {
        //            Item trashItem = player[0].TPlayer.trashItem;

        //            string msg = "Trash: " +
        //                trashItem.Name ?? "Empty" +
        //                "\n\nPiggy:";


        //            Chest piggyBank = player[0].TPlayer.bank;
        //            Chest safeBank = player[0].TPlayer.bank2;
        //            foreach (Item pig in piggyBank.item)
        //            {
        //                if (pig.Name == null || pig.Name == "")
        //                    continue;
        //                msg += "\n`" + pig.Name + "`" + " - " + pig.stack;
        //            }

        //            msg += "\n \n" + "Safe:";
        //            foreach (Item safe in safeBank.item)
        //            {
        //                if (safe.Name == null || safe.Name == "")
        //                    continue;
        //                msg += "\n`" + safe.Name + "`" + " - " + safe.stack;
        //            }

        //            var emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
        //            var embed = new DiscordEmbedBuilder
        //            {
        //                Title = $"{emoji} **" + username + "**",
        //                Description = msg,
        //                Color = new DiscordColor(0xAC983C)
        //            };
        //            await ctx.RespondAsync("", embed: embed);
        //            return;
        //        }
        //        else
        //        {
        //            var emoji = DiscordEmoji.FromName(ctx.Client, ":warning:");
        //            var embed = new DiscordEmbedBuilder
        //            {
        //                Title = "Игрок не найден!",
        //                Description = $"{emoji} По запросу `{username}` не найдено ни одного игрока!",
        //                Color = new DiscordColor(0xFF0000) // red
        //                                                   // there are also some pre-defined colors available
        //                                                   // as static members of the DiscordColor struct
        //            };
        //            await ctx.RespondAsync("", embed: embed);
        //            return;
        //        }
        //    }
        //    else
        //    {
        //        var emojiplr = DiscordEmoji.FromName(ctx.Client, ":warning:");
        //        var embedplr = new DiscordEmbedBuilder
        //        {
        //            Title = "Неправильный синтаксис!",
        //            Description = $"{emojiplr} /inv <account/username>",
        //            Color = new DiscordColor(0xFF0000) // red
        //                                               // there are also some pre-defined colors available
        //                                               // as static members of the DiscordColor struct
        //        };
        //        await ctx.RespondAsync("", embed: embedplr);
        //    }
        //}
    }
}
