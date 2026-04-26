using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content
{
	public class VerminLordModSystem : ModSystem
	{
		public override void PostUpdateEverything() {
			LiquidTrailManager.UpdateAll();
		}

		public override void PostDrawInterface(SpriteBatch sb) {
			LiquidTrailManager.DrawAll(sb);
		}
	}
}
