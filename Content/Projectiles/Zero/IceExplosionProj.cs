using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 冰道冰爆术 — 范围冰爆弹幕。
    ///
    /// 行为组合：
    /// - ParticleBodyBehavior: 冰色粒子体
    /// - IceTrailBehavior: 冰系拖尾（十字星 + 雪片）
    /// - AimBehavior: 直线飞行
    /// - KillOnContactBehavior: 碰到物块/敌人时销毁，触发爆炸
    /// - SuppressDrawBehavior: 隐藏默认贴图
    /// - OnKilled: 巨大冰爆炸（IceFragmentProj + 暴风雪粒子）
    /// </summary>
    public class IceExplosionProj : BaseBullet
    {
        private const float FlySpeed = 9f;
        private const float BlastRadius = 140f;

        private const int FragmentBurstCount = 20;
        private const float FragmentBurstSpeed = 7f;

        private const int BlizzardStarCount = 30;
        private const int BlizzardSnowCount = 50;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 14, bodyRadius: 12f)
            {
                ParticleSize = 0.6f,
                ColorStart = new Color(160, 220, 255, 220),
                ColorEnd = new Color(160, 220, 255, 220),
                SwirlSpeed = 0.03f,
                ReturnForce = 0.5f,
                JitterStrength = 0.15f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.3f,
                EnableLight = false,
            });

            Behaviors.Add(new IceTrailBehavior
            {
                MaxStars = 30,
                StarLife = 35,
                StarSpawnInterval = 2,
                StarSize = 0.7f,
                MaxSnowflakes = 90,
                SnowflakeLife = 30,
                SnowflakeSize = 0.35f,
                SnowflakeClusterSize = 5,
                SnowflakeSpawnChance = 0.7f,
                SnowflakeGravity = 0.1f,
                AutoDraw = true,
                SuppressDefaultDraw = false,
            });

            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 碰到物块或敌人时立即销毁，触发 OnKill → OnKilled 爆炸
            Behaviors.Add(new KillOnContactBehavior());

            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.2f;
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
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.8f);
        }

        protected override void OnKilled(int timeLeft)
        {
            Player owner = Main.player[Projectile.owner];
            IEntitySource source = owner?.GetSource_FromThis();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= BlastRadius)
                    {
                        npc.AddBuff(BuffID.Frostburn, 180);
                        npc.AddBuff(BuffID.Slow, 240);
                        npc.AddBuff(BuffID.Chilled, 120);
                        npc.AddBuff(BuffID.Frozen, 30);

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

            int fragType = ModContent.ProjectileType<IceFragmentProj>();
            for (int i = 0; i < FragmentBurstCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, FragmentBurstSpeed);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Projectile.NewProjectile(
                    source,
                    Projectile.Center + Main.rand.NextVector2Circular(15f, 15f),
                    vel,
                    fragType,
                    0,
                    0f,
                    Projectile.owner
                );
            }

            int starDustType = ModContent.DustType<IceBlizzardStarDust>();
            for (int i = 0; i < BlizzardStarCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(1f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    starDustType,
                    vel,
                    0,
                    new Color(180, 230, 255, 200),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
            }

            int snowDustType = ModContent.DustType<IceBlizzardSnowDust>();
            for (int i = 0; i < BlizzardSnowCount; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 8f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                vel.Y -= Main.rand.NextFloat(1f, 4f);

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(35f, 35f),
                    snowDustType,
                    vel,
                    0,
                    new Color(200, 240, 255, 180),
                    Main.rand.NextFloat(0.5f, 1.1f)
                );
                d.noGravity = false;
            }
        }
    }
}