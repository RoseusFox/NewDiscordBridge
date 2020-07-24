<p align="center">
	<a href="https://discord.gg/myapppE">
    		<img src="https://img.shields.io/discord/565583751665549344?color=%237289da&label=Discord" />
 	</a>
	<br/><br/>
  	<a href="https://github.com/Mortmael/NewDiscordBridge/blob/master/README.md">English</a>
</p>

NewDiscordBridge - это новый плагин, который позволит вам не только обмениваться сообщения между сервером и дискордом, но и модерировать не выходя из дискорда!

Поддерживает: [Оригинальный TShock](https://github.com/Pryaxis/TShock), [Мобильный TShock](https://github.com/Fe7n/TShockMobile)

## Commands
* **ban <player/IP> \<time> [reason]** - банит пользователя, или IP. **Необходимо право на бан!**
	* **unban <player/IP>**
* **kick \<player> [reason]** - кикает игрока.  **Необходимо право на кик!**
* **mute \<player>** - мутит, или снимает с игрока.  **Необходимо право на мут в голосовых чатах!**
* **off** - выключает сервер, сохраняя мир и SSC.  **Необходимо право администратора сервера!**
* **info <player/IP>** - информация по нику, или IP.
* **who** - список игроков на сервере.

## Настройки (NewDiscordBridge.json)
Name | Description | Type
-----| ------------|------
DiscordBotToken | Сюда вставьте токен вашего бота с сайта [Developer Portal](https://discord.com/developers/) | String
Prefix | Префикс для команд, например: "/", или "bot-" | String
Chat | Включите, если вам нужен чат между сервером и дискордом | Boolean
Commands | Включите, если вам нужны команды | Boolean
ChatID | ID канала для чата | UInt64
LogID | ID канала для логов | UInt64
JoinLogID | ID канала для сообщений входа/выхода | UInt64
DiscordToTerrariaFormat | Формат сообщений **из** дискорда. <br/>*{0} - Ник, {1} - Текст сообщения* | String
TerrariaToDiscordFormat | Формат сообщений **из** сервера. <br/>*{0} - Префикс, {1} - Ник, {2} - Суффикс, {3} - Текст сообщения* | String
Messagecolor | Цвет в формате RGB для сообщений **из** дискорда | Int32[]
