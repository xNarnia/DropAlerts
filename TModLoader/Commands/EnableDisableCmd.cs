using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Chat;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using static DropAlerts.DropAlerts;
using DropAlerts.Models;

namespace DropAlerts.Commands
{
	public class EnableDropAlertCmd : EnableDisableBaseCmd
	{
		public override string Command => "dropon";
		public override string Description => DescriptionLocalizedText.Value;
		public override bool SetEnabled => true;

		public override void SetStaticDefaults()
		{
			DescriptionLocalizedText = Mod.GetLocalization($"Commands.EnableDropAlertCmd.Description", () => "Enable drop alert.");
		}
	}

	public class DisableDropAlertCmd : EnableDisableBaseCmd
	{
		public override string Command => "dropoff";
		public override string Description => DescriptionLocalizedText.Value;
		public override bool SetEnabled => false;
		
		public override void SetStaticDefaults()
		{
			DescriptionLocalizedText = Mod.GetLocalization($"Commands.DisableDropAlertCmd.Description", () => "Disable drop alert.");
		}
	}

	public abstract class EnableDisableBaseCmd : ModCommand
	{
		public override CommandType Type => CommandType.World;
		public override string Usage => "/" + Command + " 1/2/3/4";
		public abstract bool SetEnabled { get; }
		public LocalizedText DescriptionLocalizedText { get; set; }

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			string message = input.Split(" ", 2)[1];

			if (int.TryParse(message, out int rarityToChange))
			{
				if (rarityToChange < 1 || rarityToChange > 4)
				{
					SendErrorMessage(message, caller.Player.whoAmI);
					return;
				}

				var playerData = DAMod.PlayerStorage.GetValueOrDefaultFromPlayer(caller.Player);
				playerData.DropAlerts.SetDropAlert(rarityToChange, SetEnabled);
				DAMod.PlayerStorage.SetValue(caller.Player, playerData);
				DAMod.PlayerStorage.Save();

				string rarityString = $"[C/FFF014:<{ModClientDisplayName}>] " + string.Format(Language.GetTextValue("Bestiary_ItemDropConditions.SimpleCondition"), string.Concat(Enumerable.Repeat(RarityItemIcon, rarityToChange)));

				rarityString = SetEnabled
					? $"{rarityString} {Language.GetTextValue("GameUI.Enabled")}"
					: $"{rarityString} {Language.GetTextValue("GameUI.Disabled")}";

				ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(rarityString), Color.White, caller.Player.whoAmI);
				Console.WriteLine(string.Format("<{0}>: Executed /{1} {2}", Main.player[caller.Player.whoAmI].name, Command, rarityToChange));
			}
			else
			{
				SendErrorMessage(message, caller.Player.whoAmI);
				return;
			}
		}

		private void SendErrorMessage(string input, int playerId)
		{
			ChatHelper.SendChatMessageToClient(NetworkText.FromLiteral(input + " is not a valid option. Use " + Usage), Color.Red, playerId);
		}
	}
}
