using DropAlerts.Models;
using System.Collections.Generic;
using System.IO;
using Terraria;

namespace DropAlerts
{
	public class DAPlayerDataStorage : SimpleConfig<DAPlayerDataStorage>
	{
		public override string FileName { get; set; }
			= Path.Combine(Plugin.ModConfigPath, "DropAlerts.data");

		public Dictionary<string, PlayerConfigOptions> Data { get; set; } = new Dictionary<string, PlayerConfigOptions>();

		public bool TryGetValueFromPlayer(Player player, out PlayerConfigOptions configOptions)
			=> Data.TryGetValue($"{player.name}:{player.GetPlayerStorageKey()}", out configOptions);

		public PlayerConfigOptions GetValueOrDefaultFromPlayer(Player player)
			=> Data.GetValueOrDefault($"{player.name}:{player.GetPlayerStorageKey()}") ?? new PlayerConfigOptions();

		public PlayerConfigOptions SetValue(Player player, PlayerConfigOptions configOptions)
			=> Data[$"{player.name}:{player.GetPlayerStorageKey()}"] = configOptions;
	}
}
