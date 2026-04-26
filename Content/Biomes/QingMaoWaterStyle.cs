using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Biomes
{
	public class QingMaoWaterStyle : ModWaterStyle
	{
		private Asset<Texture2D> rainTexture;
		public override void Load() {
			rainTexture = Mod.Assets.Request<Texture2D>("Content/Biomes/QingMaoRain");
		}

		public override int ChooseWaterfallStyle() {
			return ModContent.GetInstance<QingMaoWaterfallStyle>().Slot;
		}

		public override int GetSplashDust() {
			return DustID.ShimmerSplash;
		}

		public override int GetDropletGore() {
			return ModContent.GoreType<QingMaoDroplet>();
		}

		public override void LightColorMultiplier(ref float r, ref float g, ref float b) {
			r = 1f;
			g = 1f;
			b = 1f;
		}

		public override Color BiomeHairColor() {
			return Color.White;
		}

		public override byte GetRainVariant() {
			return (byte)Main.rand.Next(3);
		}

		public override Asset<Texture2D> GetRainTexture() => rainTexture;

	}
}