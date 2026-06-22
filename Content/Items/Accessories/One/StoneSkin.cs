﻿﻿﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Accessories;

namespace VerminLordMod.Content.Items.Accessories.One
{
	class StoneSkin : GuBaseItem
	{
		public override void SetDefaults() {
			Item.width = 24;
			Item.height = 28;
			Item.value = 50000;
			Item.rare = ItemRarityID.White;
			Item.defense = 5;
			Item.useStyle = ItemUseStyleID.Guitar;
		}
	}
}
