using Terraria;

namespace DropAlerts.Models
{
	public static class Extensions
	{
		public static string GetPlayerStorageKey(this Player player)
			=> Netplay.Clients[player.whoAmI].Socket.GetRemoteAddress().GetFriendlyName().Split(":")[0]?.Base64Encode();
		
		public static string Base64Encode(this string text)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(text);
			return System.Convert.ToBase64String(bytes);
		}
	}
}
