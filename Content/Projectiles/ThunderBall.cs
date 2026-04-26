using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using VerminLordMod.Content.DamageClasses;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.Audio;
using Microsoft.Xna.Framework;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Projectiles
{
	class ThunderBall:ModProjectile
	{
		public override void SetDefaults() {
			Projectile.width = 16; // 弹幕的碰撞箱宽度
			Projectile.height = 16; // 弹幕的碰撞箱高度
									// 这两个字段不赋值弹幕会射不出来！16*16的碰撞箱相当于泰拉里一个物块那么大
									// 特别注意，请不要搞什么碰撞箱大小设为贴图大小的骚操作，那会造成奇怪的后果
			Projectile.scale = 1f; // 弹幕缩放倍率，会影响碰撞箱大小，默认1f
			Projectile.ignoreWater = true; // 弹幕是否忽视水
			Projectile.tileCollide = false; // 弹幕撞到物块会创死吗
			Projectile.penetrate = 99; // 弹幕的穿透数，默认1次
			Projectile.timeLeft = 9600; // 弹幕的存活时间，它会从弹幕生成开始每次更新减1，为零时弹幕会被kill，默认3600
			Projectile.alpha = 0; // 弹幕的透明度，0 ~ 255，0是完全不透明（int）
								  // Projectile.Opacity = 1; // 弹幕的不透明度，0 ~ 1，0是完全透明，1是完全不透明(float)，用哪个你们自己挑，这两是互相影响的
			Projectile.friendly = true; // 弹幕是否攻击敌方，默认false
			Projectile.hostile = false; // 弹幕是否攻击友方和城镇NPC，默认false
			Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>(); // 弹幕的伤害类型，默认default，npc射的弹幕用这种，玩家的什么类型武器就设为什么吧
																				 // Projectile.aiStyle = ProjAIStyleID.Arrow; // 弹幕使用原版哪种弹幕AI类型
																				 // AIType = ProjectileID.FireArrow; // 弹幕模仿原版哪种弹幕的行为
																				 // 上面两条，第一条是某种行为类型，可以查源码看看，这里是箭矢，第二条要有第一条才有效果，是让这个弹幕能执行对应弹幕的特殊判定行为
			//Projectile.aiStyle = ProjAIStyleID.MagnetSphere;
			Projectile.aiStyle = ProjAIStyleID.LightningOrb;

			
			// 不用原版的就写这个，也可以不写
			// Projectile.extraUpdates = 0; // 弹幕每帧的额外更新次数，默认0，这个之后细讲
			// 以及写一些关于无敌帧的设定
		}

		private int frametime = 0;
		public override void AI() {
			Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.MinecartSpark, 0f, 0f, 100, default(Color), 3f);
			// 粒子特效不受重力
			dust.noGravity = true;
			NPC npc = Finder.FindCloestEnemy(Projectile.Center, 200f, (n) => {
				return n.CanBeChasedBy() &&
				!n.dontTakeDamage && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1);
			});
			if (npc != null && frametime % 20 == 0) {
				//var s = player.GetSource_ItemUse_WithPotentialAmmo(0, 0);
				Vector2 targetPos = npc.position;
				Projectile p = Projectile.NewProjectileDirect(null, Projectile.Center, Vector2.Normalize(targetPos - Projectile.Center) * 20f, ProjectileID.ThunderSpearShot, 45, 2);

			}
			//if(frametime % 30 == 0) qiPlayer.qiCurrent-=1;
			frametime++;
			//NPC npc = Finder.FindCloestEnemy(Projectile.Center, 800f, (n) => {
			//	return n.CanBeChasedBy() &&
			//	!n.dontTakeDamage && Collision.CanHitLine(Projectile.Center, 1, 1, n.Center, 1, 1);
			//});
			//float i = 50 - player.maxMinions * 3 + player.slotsMinions * 3;
			//i = Utils.Clamp(i, 1, 50);
			//if (npc != null && frametime % i == 0) {
			//	qiPlayer.qiCurrent -= 5;
			//	Projectile p = Projectile.NewProjectileDirect(null, Projectile.Center, Vector2.UnitY, ModContent.ProjectileType<WaterBall>(), 15, 2);
			//}
			base.AI();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Electrified, 300);
		}

	}
}
