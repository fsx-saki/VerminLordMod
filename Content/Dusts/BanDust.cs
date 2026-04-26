using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Dusts
{
	public class BanDust : ModDust
	{
		
		
		public override void OnSpawn(Dust dust) {
			dust.noGravity = true;
			dust.alpha = 100;
			dust.color = Color.White;
			dust.rotation = 0;
			dust.frame = Texture2D.Frame(1, 1, 0, 0, 10, 10);
			dust.scale = 1;
		}

		public override bool Update(Dust dust) {
			//旋转每帧增加0.05f
			dust.position -= Vector2.UnitY;
			dust.alpha += 5;
			dust.fadeIn++;
			if (dust.fadeIn > 60 * 2)//设置3秒钟后消失。原版的粒子基本上都是scale小于一定值就自动消失的
				dust.active = false;//直接让粒子消失
			return false;
		}
	}
}
