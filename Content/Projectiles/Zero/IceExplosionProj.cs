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
    /// 冰道冰爆术 — 范围冰爆弹幕。
    /// 冰道技术储备库的"冰爆术"技术：
    /// - 弹幕飞行到目标位置后爆炸
    /// - 爆炸产生大范围冰霜伤害
    /// - 对范围内的敌人附加冻结效果
    /// - 产生大量冰晶粒子
    ///
    /// 行为组合：
    /// - AimBehavior: 直线飞行
    /// - DustTrailBehavior: 冰晶粒子拖尾
    /// - 自定义 OnKilled: 范围冰爆
    /// </summary>
    public class IceExplosionProj : BaseBullet
    {
        /// <summary>飞行速度</summary>
        private const float FlySpeed = 9f;

        /// <summary>爆炸范围（像素）</summary>
        private const float BlastRadius = 100f;

        protected override void RegisterBehaviors()
        {
            // 1. 直线飞行
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 2. 冰晶粒子拖尾
            Behaviors.Add(new DustTrailBehavior(DustID.Ice, spawnChance: 1)
            {
                DustScale = 0.7f,
                VelocityMultiplier = 0.15f,
                NoGravity = true,
                DustAlpha = 150,
                RandomSpeed = 0.4f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
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
            // 冰晶微光
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.8f);
        }

        protected override void OnKilled(int timeLeft)
        {
            // 爆炸时产生范围冰爆
            // 1. 伤害并冻结范围内的敌人
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= BlastRadius)
                    {
                        // 附加冻结效果
                        npc.AddBuff(BuffID.Frostburn, 180);
                        npc.AddBuff(BuffID.Slow, 240);
                        npc.AddBuff(BuffID.Chilled, 120);
                        npc.AddBuff(BuffID.Frozen, 30); // 短暂冻结

                        // 造成伤害
                        Player owner = Main.player[Projectile.owner];
                        if (owner != null && owner.active)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = Projectile.damage,
                                Knockback = 4f,
                                HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }

            // 2. 产生大量冰晶粒子（爆炸效果）
            for (int i = 0; i < 30; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(3f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    DustID.Ice,
                    vel,
                    0,
                    new Color(180, 230, 255, 200),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
            }

            // 3. 产生冰环效果
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12;
                float radius = BlastRadius * 0.5f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Ice,
                    angle.ToRotationVector2() * Main.rand.NextFloat(1f, 3f),
                    0,
                    new Color(200, 240, 255, 150),
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
                d.noGravity = true;
            }
        }
    }
}
