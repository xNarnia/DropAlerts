using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria.Chat;
using Terraria.Localization;
using TShockAPI;
using static DropAlerts.Plugin;

namespace DropAlerts.Cmds
{
	public class ShowDropAlerts : ICmd
	{
		public string Command
			=> "showdrops";

		public void Run(CommandArgs args)
		{
			var playerData = DAMod.PlayerStorage.GetValueOrDefaultFromPlayer(args.Player.TPlayer);
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

			ChatHelper.SendChatMessageToClient(NetworkText.FromFormattable(output), new Color(255, 240, 20), args.Player.TPlayer.whoAmI);
			Console.WriteLine(string.Format("<{0}> called /" + Command, Terraria.Main.player[args.Player.TPlayer.whoAmI].name));
		}
	}
}
