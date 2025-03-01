using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Localization;
using TShockAPI;
using static DropAlerts.Plugin;

namespace DropAlerts.Cmds
{
	public class EnableDropAlertCmd : EnableDisableCmd, ICmd
	{
		public override string Command => "dropon";
		public override bool SetEnabled => true;
	}

	public class DisableDropAlertCmd : EnableDisableCmd, ICmd
	{
		public override string Command => "dropoff";
		public override bool SetEnabled => false;
	}

	public abstract class EnableDisableCmd
	{
		public abstract string Command { get; }
		public abstract bool SetEnabled { get; }

		public void Run(CommandArgs args)
		{
			string message = args.Message.Split(" ", 2)[1];

			if (int.TryParse(message, out int rarityToChange))
			{
				if (rarityToChange < 1 || rarityToChange > 4)
				{
					SendErrorMessage(message, args.Player);
					return;
				}

				var playerData = DAMod.PlayerStorage.GetValueOrDefaultFromPlayer(args.Player.TPlayer);
				playerData.DropAlerts.SetDropAlert(rarityToChange, SetEnabled);
				DAMod.PlayerStorage.SetValue(args.Player.TPlayer, playerData);
				DAMod.PlayerStorage.SaveJson();

				string rarityString = $"[C/FFF014:<{ModClientDisplayName}>] " + string.Format(Language.GetTextValue("UI.EmoteCategoryItems"), string.Concat(Enumerable.Repeat(RarityItemIcon, rarityToChange)));

				rarityString = SetEnabled
					? $"{rarityString} {Language.GetTextValue("GameUI.Enabled")}"
					: $"{rarityString} {Language.GetTextValue("GameUI.Disabled")}";

				args.Player.SendMessage(rarityString, Color.White);
				Console.WriteLine(string.Format("<{0}>: Executed /{1} {2}", args.Player.Name, Command, rarityToChange));
			}
			else
			{
				SendErrorMessage(message, args.Player);
				return;
			}
		}

		public void SendErrorMessage(string msg, TSPlayer player)
		{
			player.SendMessage(msg + " is not a valid option.", Color.Red);
		}
	}
}
