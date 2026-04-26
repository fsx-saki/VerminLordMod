using VerminLordMod.Content.Biomes;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Tiles
{
	public class QingMaoStoneBlock : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileSolid[Type] = true;
			Main.tileMergeDirt[Type] = true;
			Main.tileBlockLight[Type] = true;

			//DustType = ModContent.DustType<Sparkle>();

			AddMapEntry(new Color(200, 200, 200));
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}

		public override void ChangeWaterfallStyle(ref int style) {
			style = ModContent.GetInstance<QingMaoWaterfallStyle>().Slot;
		}
	}
}