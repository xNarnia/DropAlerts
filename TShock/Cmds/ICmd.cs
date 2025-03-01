using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace DropAlerts.Cmds
{
	public interface ICmd
	{
		public string Command { get; }
		public void Run(CommandArgs args);
	}
}
