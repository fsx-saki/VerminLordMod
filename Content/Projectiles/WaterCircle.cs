using VerminLordMod.Common.Players;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static VerminLordMod.Content.Items.Weapons.One.RiverStream;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Projectiles
{
	class WaterCircle:ModProjectile
	{
		public static Player player;
		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 3;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.scale = 1.1f;
			// 召唤物必备的属性
			Main.projPet[Type] = true;
			Projectile.netImportant = true;
			Projectile.minionSlots = 1;
			Projectile.minion = true;
			ProjectileID.Sets.MinionSacrificable[Type] = true;
			ProjectileID.Sets.CultistIsResistantTo[Type] = true;
		}

		private int frametime = 0;
		public override void AI() {
			var modPlayer = player.GetModPlayer<Bais>();
			var qiResource = player.GetModPlayer<QiResourcePlayer>();
			// 玩家死亡会让召唤物消失
			if (player.dead || qiResource.QiCurrent == 0) {
				modPlayer.WaterCircle = false;
			}
			if (modPlayer.WaterCircle) {
				// 如果Gliders不为true那么召唤物弹幕只有两帧可活
				Projectile.timeLeft = 2;
			}
			MoveAroundPlayer(player);
			NPC npc = Finder.FindCloestEnemy(Projectile.Center, 800f, (n) =>
			{
				return n.CanBeChasedBy() &&
				!n.dontTakeDamage && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1);
			});
			float i = 50 - player.maxMinions * 3 + player.slotsMinions * 3;
			i = Utils.Clamp(i, 1, 50);
			if (npc != null&& frametime % i == 0) {
				qiResource.ConsumeQi(5);
				Projectile p = Projectile.NewProjectileDirect(null, Projectile.Center, Vector2.UnitY, ModContent.ProjectileType<WaterBall>(),15,2);
			}
			//if(frametime % 30 == 0) qiPlayer.qiCurrent-=1;
			frametime++;
		}


		private void MoveAroundPlayer(Player player)
        {
			Vector2 offset = new Vector2(-24,0);
			Vector2 diff = player.Center-Projectile.Center+offset;
			Projectile.position += diff;
        }


		public override void OnSpawn(IEntitySource source) {
			player = Main.player[Projectile.owner];
			var modPlayer = player.GetModPlayer<Bais>();
			player.AddBuff(ModContent.BuffType<WaterCirclebuff>(), 2);// 把之前说的添加buff放在这里
		}

	}
}
