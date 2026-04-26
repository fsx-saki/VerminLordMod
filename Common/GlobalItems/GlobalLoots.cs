using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Weapons.One;

namespace VerminLordMod.Common.GlobalItems
{
	class GlobalLoots:GlobalTile
	{
		//public override void Drop(int i, int j, int type) {
		//	Main.NewText("loot");
		//	Item.NewItem(new EntitySource_TileBreak(i, j), i, j, 16, 16, ModContent.ItemType<DirtGu>());
		//	Item.NewItem(new EntitySource_TileBreak(i, j), i, j, 16, 16, type);
		//	//base.Drop(i, j, type);
		//}
		//public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
		//	Main.NewText("loot");
		//	Item.NewItem(new EntitySource_TileBreak(i, j), i, j, 16, 16, ModContent.ItemType<DirtGu>());
		//	base.KillTile(i, j, type, ref fail, ref effectOnly, ref noItem);
		//}
	}
}
