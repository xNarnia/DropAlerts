using Newtonsoft.Json;
using System.Collections.Generic;

namespace DropAlerts.Models
{
	public class PlayerConfigOptions
	{
		[JsonProperty("i")]
		public List<string> IgnoreItems { get; set; } = new List<string>();

		[JsonProperty("m")]
		public PlayerDropAlertInfo DropAlerts { get; set; } = new PlayerDropAlertInfo();
	}
}
