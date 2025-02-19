using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace DropAlerts
{
	public class DAModConfig : ModConfig
	{
		public const float MaxPercent = 20f;
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[DefaultValue(true)]
		public bool Enable1StarDropNotice { get; set; }

		[DefaultValue(true)]
		public bool Enable2StarDropNotice { get; set; }

		[DefaultValue(true)]
		public bool Enable3StarDropNotice { get; set; }

		[DefaultValue(true)]
		public bool Enable4StarDropNotice { get; set; }

		[Range(0f, MaxPercent)]
		[DefaultValue(5f)]
		public float Percent1Star { get; set; }

		[Range(0f, MaxPercent)]
		[DefaultValue(3f)]
		public float Percent2Star { get; set; }

		[Range(0f, MaxPercent)]
		[DefaultValue(1f)]
		public float Percent3Star { get; set; }

		[Range(0f, MaxPercent)]
		[DefaultValue(.5f)]
		public float Percent4Star { get; set; }
	}
}
