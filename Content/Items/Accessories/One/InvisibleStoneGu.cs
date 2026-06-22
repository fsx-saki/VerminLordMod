﻿﻿﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Accessories;

namespace VerminLordMod.Content.Items.Accessories.One
{
	class InvisibleStoneGu : GuBaseItem
	{
		protected override int qiCost => 5;

		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.value = 50000;
			Item.rare = ItemRarityID.White;
			Item.defense = 0;
			Item.useStyle = ItemUseStyleID.Guitar;
		}
	}
}
