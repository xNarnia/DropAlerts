using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using Terraria.Net;
using static Terraria.NetMessage;
using Microsoft.Xna.Framework;
using Terraria.GameContent.NetModules;
using DropAlerts.Models;

namespace DropAlerts
{
	public class DropAlerts : Mod
	{
		public static DropAlerts DAMod { get; set; }
		public DAModConfig Config => ModContent.GetInstance<DAModConfig>();
		public DAPlayerDataStorage PlayerStorage => ModContent.GetInstance<DAPlayerDataStorage>();

		// Consts
		public const string ModClientDisplayName = "Drop Alerts";
		public const string RarityItemIcon = "[i/s1:75]";

		public override void Load()
		{
			base.Load();
			DAMod = this;
			On_ItemDropResolver.ResolveRule += NotifyDrop;
			On_NetMessage.greetPlayer += NetMessage_greetPlayer;
		}

		public override void Unload()
		{
			base.Unload();
			DAMod = null;
			On_ItemDropResolver.ResolveRule -= NotifyDrop;
			On_NetMessage.greetPlayer -= NetMessage_greetPlayer;
		}

		private void NetMessage_greetPlayer(On_NetMessage.orig_greetPlayer orig, int plr)
		{
			var helpCmdString = Language.GetTextValue("ChatCommandDescription.Help");
			helpCmdString = helpCmdString.Replace(Language.GetTextValue("ChatCommand.Help"), "");
			NetPacket packet = NetTextModule.SerializeServerMessage(NetworkText.FromLiteral($"[{ModClientDisplayName}]\n  /help" + helpCmdString), Color.SkyBlue, byte.MaxValue);
			NetManager.Instance.SendToClient(packet, plr);
			orig(plr);
		}

		// Code inspired by https://github.com/BugraPearls/Rare-Drop-Notification. Thanks Pearlie for your permission to use!
		private ItemDropAttemptResult NotifyDrop(On_ItemDropResolver.orig_ResolveRule orig, ItemDropResolver self, IItemDropRule rule, DropAttemptInfo info)
		{
			ItemDropAttemptResult result = orig(self, rule, info);
			if (result.State is ItemDropAttemptResultState.Success && rule is CommonDrop drop)
			{
				double chance = Math.Round((double)Math.Max(drop.chanceNumerator, 1) / Math.Max(drop.chanceDenominator, 1) * 100, 3);

				if (chance <= DAModConfig.MaxPercent)
				{
					try
					{
						Color textColor;
						ushort soundIndex;
						float pitch = -1;
						string star = RarityItemIcon;

						if (chance <= Config.Percent4Star && Config.Enable4StarDropNotice)
						{
							textColor = new Color(255, 80, 80);
							soundIndex = 305; // Razorfish Typhoon use
							pitch = .2f;
							star = star + star;
						}
						else if (chance <= Config.Percent3Star && Config.Enable3StarDropNotice)
						{
							textColor = new Color(175, 75, 255);
							soundIndex = 244; // Mana Crystal use
							pitch = .10f;
							star = star + star;
						}
						else if (chance <= Config.Percent2Star && Config.Enable2StarDropNotice)
						{
							textColor = new Color(255, 0, 160);
							soundIndex = 244; // Mana Crystal use
						}
						else if (chance <= Config.Percent1Star && Config.Enable1StarDropNotice)
						{
							textColor = new Color(255, 240, 20);
							soundIndex = 256; // Life Crystal use
						}
						else
						{
							return result;
						}

						// Message => "* Lucky * Drops: [ItemNameLink] (5%)"
						NetPacket packet = NetTextModule.SerializeServerMessage(
							NetworkText.FromFormattable(
								string.Format(Language.GetTextValue("CLI.ServerMessage"), 
									$"{star} {Language.GetTextValue("Prefix.Lucky")} {star} " + 
										string.Format($"[c/FFFFFF:{info.player.name} " + Language.GetTextValue("Bestiary_ItemDropConditions.SimpleCondition"),
											"" + ContentSamples.ItemsByType[drop.itemId].Name) + $"] [i/x:{drop.itemId}]"
								)), textColor, byte.MaxValue);

						for (var i = 0; i < Main.player.Length; i++)
						{
							if(Main.player[i].name != "") // Do not send to invalid players
							{
								PlayerDropAlertInfo dropAlerts = null;
								if (PlayerStorage.TryGetValueFromPlayer(Main.player[i], out var outMuteInfo))
								{
									dropAlerts = outMuteInfo.DropAlerts;
								}
								dropAlerts = dropAlerts ?? new PlayerDropAlertInfo();

								// Send an alert to all nearby players if their alert for this drop are enabled
								if (!dropAlerts.AlertForDropRateIsDisabled((float)chance))
								{
									var distance = Main.player[i].Distance(info.player.position);
									if (distance < 3000)
									{
										NetManager.Instance.SendToClient(packet, i);
										var pos = info.player.position;
										PlayNetSound(new NetSoundInfo(pos, soundIndex, -1, -1, pitch), -1, -1);
									}
								}
							}
						}
					}
					catch(Exception e)
					{
						Console.WriteLine($"[{ModClientDisplayName}] error!");
						Console.WriteLine(e);
						return result;
					}
				}
			}
			return result;
		}
	}
}
