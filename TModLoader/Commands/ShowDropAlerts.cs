using DropAlerts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using static DropAlerts.DropAlerts;
using Microsoft.Xna.Framework;

namespace DropAlerts.Commands
{
	public class ShowDropAlerts : ModCommand
	{
		public override CommandType Type
			=> CommandType.World;

		public override string Command
			=> "showdrops";

		public override string Usage
			=> "/" + Command;

		public override string Description => DescriptionLocalizedText.Value;

		public LocalizedText DescriptionLocalizedText { get; set; }

		public override void SetStaticDefaults()
		{
			DescriptionLocalizedText = Mod.GetLocalization($"Commands.ShowDropAlerts.Description", () => "Show current drop alerts.");
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			var playerData = DAMod.PlayerStorage.GetValueOrDefaultFromPlayer(caller.Player);
			var output = "\n" + ModClientDisplayName + ":";

			for (var i = 1; i <= playerData.DropAlerts.Count(); i++)
			{
				output += $"\n";

				output += string.Concat(Enumerable.Repeat("[i/x:97]", playerData.DropAlerts.Count() - i));
				output += string.Concat(Enumerable.Repeat(RarityItemIcon, i));

				var rarityString = playerData.DropAlerts.GetDropAlert(i) 
					? "[i/x:546][c/FFFFFF:" + Language.GetTextValue("GameUI.Enabled") + "]"
					: "[i/x:97][c/FFFFFF:" + Language.GetTextValue("GameUI.Disabled") + "]";

				output += $" {rarityString} ({playerData.DropAlerts.GetDropRateFor(i)}%)";
			}

			ChatHelper.SendChatMessageToClient(NetworkText.FromFormattable(output), new Color(255, 240, 20), caller.Player.whoAmI);
			Console.WriteLine(string.Format("<{0}> called /" + Command, Main.player[caller.Player.whoAmI].name));
		}
	}
}
