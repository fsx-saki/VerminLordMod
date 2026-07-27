using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Common.PRTTypes;
using InnoVault.PRT;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.QuestItems;
using Microsoft.Xna.Framework.Graphics;

namespace VerminLordMod.Content.Projectiles.Elements
{
	[ElementInfo("M", "月刃")]
	public class MoonBaseProj : BaseBullet
	{
		protected override void RegisterBehaviors() {
			Behaviors.Add(new AimBehavior(0f)
            {
                AutoRotate = true, RotationOffset = MathHelper.PiOver2,
            });
		}
        public override void SetDefaults()
        {
            Projectile.width = 14; Projectile.height = 14; Projectile.scale = 1f;
            Projectile.timeLeft = 120;      // 存活 120 帧（2 秒）
            Projectile.penetrate = 99;       // 穿透 99 次（几乎无限，由 OnTileCollided 控制）
            Projectile.ignoreWater = false;   // 受水影响
            Projectile.tileCollide = true;   // 与物块碰撞
            Projectile.friendly = true;      // 对敌人造成伤害
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>(); // 蛊术伤害类型
            Projectile.aiStyle = -1;         // 禁用原版 AI
        }
        private Vector2 offset = new Vector2(16, 0);
        private Texture2D Texturew = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/Elements/MoonBaseProj").Value;
        
        protected override void OnAI()
        {
            Main.NewText(Texturew.Size().X);
            // Main.rand 在服务器端为 null，需要判空
            if (Main.rand != null)
            {
                var p = PRTLoader.NewParticle<PRT_MoonTrail>(Projectile.Center+offset-Vector2.Normalize(Projectile.velocity)*Texturew.Size().Y/2f,
                    Projectile.velocity * -0.15f,
                    new Color(255, 200, 80), Texturew.Size().X*0.9f);
                // 设置粒子存活帧数（随机范围增加变化）
                if (p != null) p.Lifetime = 40 + Main.rand.Next(6);

                if (Main.rand.NextBool(2))
                {
                    // 小尺寸
                    var s = PRTLoader.NewParticle<PRT_MoonSparkle>(Projectile.Center+offset,
                        new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-4f, 4f)),
                        new Color(255, 180, 50), Main.rand.NextFloat(0.1f, 0.3f));
                    if (s != null) s.Lifetime = 40 + Main.rand.Next(10);
                }
            }
        }
	}

}
