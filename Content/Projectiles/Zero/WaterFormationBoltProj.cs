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
    /// 水道法阵汇聚水弹 — 从法阵四周飞向法阵中心的水弹。
    ///
    /// 继承 ConvergeProjectile 基类，自动追踪 WaterFormationProj。
    /// - 从法阵边缘随机位置生成，飞向法阵中心
    /// - 到达法阵中心时产生水花爆裂 + 范围伤害
    /// - 路径上可穿透敌人造成伤害
    /// - 如果法阵提前消失，则自由飞散减速消失
    /// - 本体由粒子组成（ParticleBodyBehavior），不绘制贴图
    ///
    /// 行为组合：
    /// - ConvergeProjectile: 汇聚追踪基类
    /// - ParticleBodyBehavior: 粒子水球本体
    /// - WaterTrailBehavior: 水系拖尾
    /// - SuppressDrawBehavior: 阻止默认贴图绘制
    /// </summary>
    public class WaterFormationBoltProj : ConvergeProjectile
    {
        protected override int TargetProjType => ModContent.ProjectileType<WaterFormationProj>();

        protected override float ConvergeDistance => 18f;
        protected override float MinSpeed => 5f;
        protected override float MaxSpeed => 14f;
        protected override float LerpFactor => 0.1f;
        protected override float FreeFlightDrag => 0.96f;
        protected override float FreeFlightKillSpeed => 0.8f;
        protected override float MaxTrackRange => 600f;

        /// <summary>汇聚完成时是否对范围内敌人造成伤害</summary>
        private const float ConvergeDamageRadius = 60f;

        /// <summary>汇聚完成时伤害倍率</summary>
        private const float ConvergeDamageMultiplier = 1.2f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 12, bodyRadius: 8f)
            {
                ParticleSize = 0.8f,
                ColorStart = new Color(30, 100, 210, 220),
                ColorEnd = new Color(30, 100, 210, 220),
                SwirlSpeed = 0.1f,
                ReturnForce = 0.8f,
                JitterStrength = 0.5f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.5f,
                EnableLight = false,
            });

            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 15,
                ParticleLife = 8,
                SizeMultiplier = 0.35f,
                SpawnChance = 0.5f,
                SplashSpeed = 0.2f,
                SplashAngle = 0.15f,
                InertiaFactor = 0.03f,
                RandomSpread = 0.6f,
                Gravity = 0.15f,
                AirResistance = 0.97f,
                BubbleChance = 0.4f,
                BubbleSizeMultiplier = 1.5f,
                ColorStart = new Color(30, 100, 210, 220),
                ColorEnd = new Color(30, 100, 210, 0),
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.scale = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            base.OnSpawned(source);
            // 初始速度方向指向法阵中心
            if (TargetProjIndex >= 0 && TargetProjIndex < Main.maxProjectiles)
            {
                Projectile target = Main.projectile[TargetProjIndex];
                if (target.active && target.type == TargetProjType)
                {
                    Vector2 toTarget = target.Center - Projectile.Center;
                    if (toTarget != Vector2.Zero)
                    {
                        Projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * MinSpeed;
                        Projectile.rotation = Projectile.velocity.ToRotation();
                    }
                }
            }
        }

        // OnAI 不能重写（ConvergeProjectile.OnAI 是 sealed）
        // 粒子效果和光照通过重写 UpdateTracking 和 UpdateFreeFlight 实现

        /// <summary>
        /// 追踪模式下每帧调用（在 ConvergeProjectile 追踪逻辑之后）
        /// </summary>
        protected override void UpdateTracking(Projectile target)
        {
            base.UpdateTracking(target);

            // 飞行时产生水珠拖尾粒子
            if (Main.rand.NextBool(2))
            {
                Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(4f, 4f);
                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Water,
                    -Projectile.velocity * Main.rand.NextFloat(0.1f, 0.3f) + Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0,
                    new Color(60, 180, 255, 180),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = true;
            }

            // 微弱水光
            Lighting.AddLight(Projectile.Center, 0.05f, 0.15f, 0.3f);
        }

        /// <summary>
        /// 汇聚完成时：产生水花爆裂 + 范围伤害
        /// </summary>
        protected override void OnConverge(Projectile target)
        {
            Vector2 center = target.Center;

            // 1. 范围伤害
            int damage = (int)(Projectile.damage * ConvergeDamageMultiplier);
            Player owner = Main.player[Projectile.owner];
            if (owner != null && owner.active)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                    {
                        float dist = Vector2.Distance(center, npc.Center);
                        if (dist <= ConvergeDamageRadius)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = damage,
                                Knockback = 2f,
                                HitDirection = npc.Center.X > center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }

            // 2. 水花爆裂粒子效果
            for (int i = 0; i < 10; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Water,
                    vel,
                    0,
                    new Color(80, 200, 255, 200),
                    Main.rand.NextFloat(0.6f, 1.2f)
                );
                d.noGravity = true;
            }

            // 3. 少量水滴飞溅（有重力）
            for (int i = 0; i < 4; i++)
            {
                Vector2 vel = new Vector2(
                    Main.rand.NextFloat(-2f, 2f),
                    Main.rand.NextFloat(-3f, -1f)
                );
                Dust d = Dust.NewDustPerfect(
                    center + Main.rand.NextVector2Circular(5f, 5f),
                    DustID.Water,
                    vel,
                    50,
                    new Color(60, 180, 255, 180),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = false;
            }
        }

        /// <summary>
        /// 自由飞行时（法阵已消失）产生消散粒子
        /// </summary>
        protected override void UpdateFreeFlight()
        {
            base.UpdateFreeFlight();

            // 消散粒子
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(3f, 3f),
                    DustID.Water,
                    Main.rand.NextVector2Circular(0.5f, 0.5f),
                    0,
                    new Color(60, 180, 255, 100),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = true;
            }
        }
    }
}
