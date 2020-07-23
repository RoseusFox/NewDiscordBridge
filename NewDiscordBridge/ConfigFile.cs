using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.CLI;

namespace Terraria4PDA.DiscordBridge
{
    public class ConfigFile
    {
        // Config variables here:
        public string DiscordBotToken = "Token here";
        public string Prefix = "Prefix here";
        public ulong ChatID = 0;
        public ulong LogID = 0;
        public ulong JoinLogID = 0;


        public bool Chat = true;
        public bool Commands = true;


        public List<ulong> OffRoles = new List<ulong>();

        public List<ulong> BanRoles = new List<ulong>();

        public List<ulong> KickRoles = new List<ulong>();
        public List<ulong> MuteRoles = new List<ulong>();

        public List<ulong> ListRoles = new List<ulong>();
        public List<ulong> InfoRoles = new List<ulong>();
        public List<ulong> SafeRoles = new List<ulong>();

        public string DiscordToTerrariaFormat = "[Discord] {0}: {1}";
        public string TerrariaToDiscordFormat = "{0} {1} {2}: {3}";
        public int[] Messagecolor = { 0, 102, 204 };


        public static ConfigFile Read(string path)
        {
            if (!File.Exists(path))
            {
                ConfigFile config = new ConfigFile();

                File.WriteAllText(path, JsonConvert.SerializeObject(config, Formatting.Indented));
                return config;
            }
            return JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(path));
        }
    }
}
