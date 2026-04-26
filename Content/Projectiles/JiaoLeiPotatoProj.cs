using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
	class JiaoLeiPotatoProj : ModProjectile
	{

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.penetrate = -1; // 无限穿透
			Projectile.timeLeft = 6000; // 10秒后消失
			Projectile.tileCollide = true;
		}
		private bool HasOnTileCollide = false;
		public override bool OnTileCollide(Vector2 oldVelocity) {
			HasOnTileCollide=true;
			Projectile.velocity = Vector2.Zero;
			Projectile.position.Y -= 3; // 确保地雷在地面上
			return false; // 不消失
		}

		public override void AI() {
			// 检查是否有敌人碰到地雷
			for (int i = 0; i < Main.npc.Length; i++) {
				NPC npc = Main.npc[i];
				if (npc.active && !npc.friendly && npc.Hitbox.Intersects(Projectile.Hitbox)) {
					Explode();
					break;
				}
			}
			Projectile.velocity.Y += 0.1f; // 增加向下的速度，模拟重力加速度
			if (HasOnTileCollide) {
				Projectile.velocity.Y = 0; // 停止下落
			}
			Projectile.position += Projectile.velocity; // 更新位置
		}




		private void Explode() {
			// 创建爆炸效果
			Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ProjectileID.Grenade, 100, 10f, Main.myPlayer).alpha=254;
			Projectile.Kill();
		}
	}
}
