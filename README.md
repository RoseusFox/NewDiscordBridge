# NewDiscordBridge
<p align="center">
<a href="https://discord.gg/myapppE">
    <img src="https://img.shields.io/discord/565583751665549344?color=%237289da&label=Discord" />
 	</a>
	<!--<br/><br/>
  <a href="https://github.com/Mortmael/NewDiscordBridge//README_ru.md">Русский</a>-->
  </p>

NewDiscordBridge is a new plugin that will allow you not only to exchange messages with a chat on your server, but also
moderate directly from discord!

## Commands
* **ban <player/IP> \<time> [reason]** - ban user or ip. **Requires the ban permission!**
	* **unban <player/IP>**
* **kick \<player> [reason]** - kicks the player.  **Requires the kick permission!**
* **mute \<player>** - mute/unmute player.  **Requires the mute permission!**
* **off** - shuts down the server while saving.  **Requires the administrator permission!**
* **info <player/IP>** - shows information about the player's account or IP.
* **who** - displays a list of players.

## Config (NewDiscordBridge.json)
Name | Description | Type
-----| ------------|------
DiscordBotToken | Insert the token for your bot from the [Developer Portal](https://discord.com/developers/) here | String
Prefix | Prefix for your commands, e. g. "/", or "bot-" | String
Chat | Enable if you need chat between server and discord | Boolean
Commands | Enable if you need commands | Boolean
ChatID | ID of channel for chat | UInt64
LogID | ID of channel for logs | UInt64
JoinLogID | ID of channel for join/leave messages | UInt64
DiscordToTerrariaFormat | Format of messages from discord. <br/>*{0} - Username, {1} - Message* | String
TerrariaToDiscordFormat | Format of messages from server. <br/>*{0} - Prefix, {1} - Username, {2} - Suffix, {3} - Message* | String
Messagecolor | RGB color for messages from discord | Int32[]
