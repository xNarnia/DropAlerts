using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI.Hooks;
using OTAPI;
using Terraria.Localization;
using Terraria.Net;
using Terraria.GameContent.NetModules;
using Microsoft.Xna.Framework;
using Terraria.GameContent.ItemDropRules;
using static Terraria.NetMessage;
using Terraria.ID;
using DropAlerts.Models;
using ModFramework;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Text.RegularExpressions;
using DropAlerts.Cmds;

namespace DropAlerts
{
	[ApiVersion(2, 1)]
	public class Plugin : TerrariaPlugin
	{
		/// <summary>
		/// Gets the author(s) of this plugin
		/// </summary>
		public override string Author => "Narnia";

		/// <summary>
		/// Gets the description of this plugin.
		/// A short, one lined description that tells people what your plugin does.
		/// </summary>
		public override string Description => "In-game player chat & sound notifications for rare item drops.";

		/// <summary>
		/// Gets the name of this plugin.
		/// </summary>
		public override string Name => ModClientDisplayName;

		/// <summary>
		/// Gets the version of this plugin.
		/// </summary>
		public override Version Version => new Version(1, 0, 0, 0);

		// Consts
		public const string ModClientDisplayName = "Drop Alerts";
		public const string RarityItemIcon = "[i/s1:75]";

		public DAModConfig Config { get; set; }
		public DAPlayerDataStorage PlayerStorage { get; set; }
		public static Plugin DAMod;
		public static string ModConfigPath;

		/// <summary>
		/// Initializes a new instance of the TestPlugin class.
		/// This is where you set the plugin's order and perfrom other constructor logic
		/// </summary>
		public Plugin(Main game) : base(game)
		{
			DAMod = this;
		}

		/// <summary>
		/// Handles plugin initialization. 
		/// Fired when the server is started and the plugin is being loaded.
		/// You may register hooks, perform loading procedures etc here.
		/// </summary>
		public override void Initialize()
		{
			LoadCommands();
			On.Terraria.GameContent.ItemDropRules.ItemDropResolver.ResolveRule += NotifyDrop;
			ServerApi.Hooks.NetGreetPlayer.Register(this, GreetPlayer);
			ModConfigPath = Path.Combine(Directory.GetCurrentDirectory(), TShock.SavePath, "DropAlerts");
			Config = new DAModConfig().GetOrCreateConfiguration();
			PlayerStorage = new DAPlayerDataStorage().GetOrCreateConfiguration();
		}

		private ItemDropAttemptResult NotifyDrop(On.Terraria.GameContent.ItemDropRules.ItemDropResolver.orig_ResolveRule orig, ItemDropResolver self, IItemDropRule rule, DropAttemptInfo info)
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
							soundIndex = 265; // Razorfish Typhoon use
							pitch = .2f;
							star = star + star;
						}
						else if (chance <= Config.Percent3Star && Config.Enable3StarDropNotice)
						{
							textColor = new Color(175, 75, 255);
							soundIndex = 204; // Mana Crystal use
							pitch = .10f;
							star = star + star;
						}
						else if (chance <= Config.Percent2Star && Config.Enable2StarDropNotice)
						{
							textColor = new Color(255, 0, 160);
							soundIndex = 204; // Mana Crystal use
						}
						else if (chance <= Config.Percent1Star && Config.Enable1StarDropNotice)
						{
							textColor = new Color(255, 240, 20);
							soundIndex = 216; // Life Crystal use
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
										string.Format($"[c/FFFFFF:{info.player.name} " + Language.GetTextValue("Game.DroppedCoins"),
											"" + ContentSamples.ItemsByType[drop.itemId].Name) + $"] [i/x:{drop.itemId}]"
								)), textColor, byte.MaxValue);

						for (var i = 0; i < Main.player.Length; i++)
						{
							if (Main.player[i].name != "") // Do not send to invalid players
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
					catch (Exception e)
					{
						Console.WriteLine($"[{ModClientDisplayName}] error!");
						Console.WriteLine(e);
						return result;
					}
				}
			}
			return result;
		}

		public void GreetPlayer (GreetPlayerEventArgs args)
		{
			NetPacket packet = NetTextModule.SerializeServerMessage(NetworkText.FromLiteral($"[{ModClientDisplayName}]\n  /showdrops  /dropon  /dropoff"), Color.SkyBlue, byte.MaxValue);
			NetManager.Instance.SendToClient(packet, args.Who);
		}

		public void LoadCommands(object caller = null)
		{
			Assembly assembly;
			if (caller == null)
				assembly = Assembly.GetExecutingAssembly();
			else
				assembly = Assembly.GetAssembly(caller.GetType());

			foreach (var type in assembly.GetTypes())
			{
				if (typeof(ICmd).IsAssignableFrom(type) && !type.IsAbstract && type.IsClass)
				{
					var command = (ICmd)Activator.CreateInstance(type);
					Commands.ChatCommands.Add(new Command(command.Command, command.Run, command.Command));
				}
			}
		}

		/// <summary>
		/// Handles plugin disposal logic.
		/// *Supposed* to fire when the server shuts down.
		/// You should deregister hooks and free all resources here.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				// Deregister hooks here
			}
			base.Dispose(disposing);
		}
	}
}