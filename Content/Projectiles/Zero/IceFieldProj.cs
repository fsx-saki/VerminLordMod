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
    /// 冰道冰霜领域 — 滞留冰霜区域弹幕。
    /// 冰道技术储备库的"冰霜领域"技术：
    /// - 在指定位置生成冰霜区域，固定不动
    /// - 由 icesnow + icestar 两种贴图的大型粒子体构成视觉效果
    /// - 持续对范围内的敌人造成伤害 + 冻结效果
    /// - 持续泼洒冰碎片（IceFragmentProj）
    /// - 持续 5 秒后消失
    ///
    /// 行为组合：
    /// - ParticleBodyBehavior ×2: icesnow + icestar 大型粒子体
    /// - StationaryBehavior: 固定不动
    /// - OnAI: 每帧检测范围内的敌人 + 减速 + 伤害 + 泼洒冰碎片
    /// </summary>
    public class IceFieldProj : BaseBullet
    {
        /// <summary>冰霜领域持续时间（帧）5秒</summary>
        private const int Duration = 300;

        /// <summary>伤害检测间隔（帧）</summary>
        private const int HitInterval = 15;

        /// <summary>伤害半径（像素）</summary>
        private const float HitRadius = 100f;

        /// <summary>减速范围（像素）</summary>
        private const float SlowRange = 120f;

        /// <summary>冰碎片泼洒间隔（帧）</summary>
        private const int FragmentInterval = 20;

        /// <summary>每次泼洒的冰碎片数量</summary>
        private const int FragmentBurst = 3;

        /// <summary>计时器</summary>
        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            // 1. icesnow 大型粒子体（雪状冰晶）
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 44, bodyRadius: 50f)
            {
                ParticleSize = 0.8f,
                ColorStart = new Color(160, 220, 255, 200),
                ColorEnd = new Color(160, 220, 255, 80),
                SwirlSpeed = 0.02f,
                ReturnForce = 0.03f,
                JitterStrength = 0.4f,
                ShrinkOverLife = true,
                StretchOnMove = false,
                EnableLight = false,
                Alpha = 0.9f,
                TexturePath = "VerminLordMod/Content/Trails/IceTrail/IceTrailSnow",
            });

            // 2. icestar 大型粒子体（星状冰晶）
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 32, bodyRadius: 40f)
            {
                ParticleSize = 0.6f,
                ColorStart = new Color(200, 240, 255, 220),
                ColorEnd = new Color(200, 240, 255, 100),
                SwirlSpeed = -0.03f,
                ReturnForce = 0.03f,
                JitterStrength = 0.4f,
                ShrinkOverLife = true,
                StretchOnMove = false,
                EnableLight = false,
                Alpha = 0.8f,
                TexturePath = "VerminLordMod/Content/Trails/IceTrail/IceTrailStar",
            });

            // 3. 固定不动
            Behaviors.Add(new StationaryBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 80;
            Projectile.height = 80;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = HitInterval * 2;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
            _timer = 0;
        }

        protected override void OnAI()
        {
            _timer++;

            // 减速范围内的敌人
            SlowEnemies();

            // 间隔伤害
            if (_timer % HitInterval == 0)
            {
                DamageEnemiesInRange();
            }

            // 间隔泼洒冰碎片
            if (_timer % FragmentInterval == 0)
            {
                SpawnFragments();
            }

            // 光照
            Lighting.AddLight(Projectile.Center, 0.15f, 0.35f, 0.75f);
        }

        /// <summary>
        /// 减速范围内的敌人
        /// </summary>
        private void SlowEnemies()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= SlowRange)
                    {
                        npc.AddBuff(BuffID.Slow, 30);
                        npc.AddBuff(BuffID.Chilled, 30);
                        npc.AddBuff(BuffID.Frostburn, 30);
                        npc.AddBuff(BuffID.Frozen, 15);
                    }
                }
            }
        }

        /// <summary>
        /// 伤害范围内的敌人
        /// </summary>
        private void DamageEnemiesInRange()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist <= HitRadius)
                    {
                        Player owner = Main.player[Projectile.owner];
                        if (owner != null && owner.active)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = Projectile.damage,
                                Knockback = 1f,
                                HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 泼洒冰碎片（IceFragmentProj）
        /// </summary>
        private void SpawnFragments()
        {
            int fragType = ModContent.ProjectileType<IceFragmentProj>();
            IEntitySource source = Main.player[Projectile.owner]?.GetSource_FromThis();

            for (int i = 0; i < FragmentBurst; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;
                vel.Y -= Main.rand.NextFloat(0.5f, 2f);

                Projectile.NewProjectile(
                    source,
                    Projectile.Center + Main.rand.NextVector2Circular(30f, 30f),
                    vel,
                    fragType,
                    0,
                    0f,
                    Projectile.owner
                );
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 消失时产生冰晶爆散
            for (int i = 0; i < 20; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 6f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Ice,
                    vel,
                    0,
                    new Color(180, 220, 255, 200),
                    Main.rand.NextFloat(0.6f, 1.4f)
                );
                d.noGravity = true;
            }

            // 消失时泼洒一波冰碎片
            SpawnFragments();
        }
    }
}
