using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
	/// <summary>
	/// 玩家特效绘制系统 - 在安全的渲染时机绘制特效
	/// </summary>
	public class PlayerEffectDrawSystem : ModSystem
	{
		public override void PostDrawInterface(SpriteBatch sb) {
			// 只为自己的玩家绘制
			Player player = Main.LocalPlayer;
			if (player == null) return;

			var effectsPlayer = player.GetModPlayer<EffectsPlayer>();
			effectsPlayer.DrawEffects(sb);
		}
	}
}
