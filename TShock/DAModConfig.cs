using DropAlerts.Models;
using System.IO;
using Newtonsoft.Json;

namespace DropAlerts
{
	public class DAModConfig : SimpleConfig<DAModConfig>
	{
		public override string FileName { get; set; } 
			= Path.Combine(Plugin.ModConfigPath, "DropAlerts.json");

		[JsonIgnore]
		public const float MaxPercent = 20f;

		public bool Enable1StarDropNotice { get; set; } = true;

		public bool Enable2StarDropNotice { get; set; } = true;

		public bool Enable3StarDropNotice { get; set; } = true;

		public bool Enable4StarDropNotice { get; set; } = true;

		public float Percent1Star { get; set; } = 5f;

		public float Percent2Star { get; set; } = 3f;

		public float Percent3Star { get; set; } = 1f;

		public float Percent4Star { get; set; } = .5f;
	}
}
