using Newtonsoft.Json;
using DropAlerts.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ModLoader.Config;
using System.Text.Json.Serialization;

namespace DropAlerts
{
	public class DAPlayerDataStorage : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		public Dictionary<string, PlayerConfigOptions> Data { get; set; } = new Dictionary<string, PlayerConfigOptions>();

		public bool TryGetValueFromPlayer(Player player, out PlayerConfigOptions configOptions)
			=> Data.TryGetValue($"{player.name}:{player.GetPlayerStorageKey()}", out configOptions);

		public PlayerConfigOptions GetValueOrDefaultFromPlayer(Player player)
			=> Data.GetValueOrDefault($"{player.name}:{player.GetPlayerStorageKey()}") ?? new PlayerConfigOptions();

		public PlayerConfigOptions SetValue(Player player, PlayerConfigOptions configOptions)
			=> Data[$"{player.name}:{player.GetPlayerStorageKey()}"] = configOptions;

		public void Save()
		{
			// in-game ModConfig saving from mod code is not supported yet in tmodloader, and subject to change, so we need to be extra careful.
			// This code only supports client configs, and doesn't call onchanged. It also doesn't support ReloadRequired or anything else.
			MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
			if (saveMethodInfo != null)
				saveMethodInfo.Invoke(null, new object[] { this });
		}
	}
}
