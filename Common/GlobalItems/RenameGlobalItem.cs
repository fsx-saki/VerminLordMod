using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace VerminLordMod.Common.GlobalItems
{
	public class RenameGlobalItem : GlobalItem
	{
		public override void SetDefaults(Item item) {
			if (item.type == ItemID.WhoopieCushion) 
			{
				item.SetNameOverride("臭屁蛊");
			}
		}
	}
}
