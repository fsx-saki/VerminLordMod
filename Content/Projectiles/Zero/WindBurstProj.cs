using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 风道风爆弹 — 范围风爆弹幕。
    /// 风道技术储备库的"风爆"技术：
    /// - 弹幕飞行到目标位置后爆炸
    /// - 爆炸产生大范围击退效果
    /// - 对范围内的敌人造成伤害
    /// - 产生大量风之粒子
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - DustTrailBehavior: 风之粒子拖尾
    /// - 自定义 OnKilled: 范围风爆
    /// </summary>
    public class WindBurstProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 10f;

        /// <summary>爆炸范围（像素）</summary>
        private const float BlastRadius = 120f;

        /// <summary>爆炸击退强度</summary>
        private const float BlastKnockback = 8f;

        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 风之粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Cloud, spawnChance: 1)
            {
                DustScale = 0.7f,
                VelocityMultiplier = 0.2f,
                NoGravity = true,
                DustAlpha = 120,
                RandomSpeed = 0.4f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            // 风之微光
            Lighting.AddLight(Projectile.Center, 0.3f, 0.5f, 0.8f);
        }

        protected override void OnKilled(int timeLeft)
        {
            // 爆炸时产生范围风爆
            // 1. 击退并伤害范围内的敌人
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= BlastRadius)
                    {
                        // 从爆炸中心向外击退
                        Vector2 pushDir = (npc.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        float pushForce = BlastKnockback * (1f - dist / BlastRadius);
                        npc.velocity += pushDir * pushForce;

                        // 造成伤害
                        Player owner = Main.player[Projectile.owner];
                        if (owner != null && owner.active)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = Projectile.damage,
                                Knockback = pushForce,
                                HitDirection = pushDir.X > 0 ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }

            // 2. 产生大量风之粒子（爆炸效果）
            for (int i = 0; i < 30; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Cloud,
                    vel,
                    0,
                    new Color(180, 220, 255, 200),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
            }

            // 3. 产生风环效果
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12;
                float radius = BlastRadius * 0.6f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Cloud,
                    angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f),
                    0,
                    new Color(200, 230, 255, 150),
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
                d.noGravity = true;
            }
        }
    }
}
