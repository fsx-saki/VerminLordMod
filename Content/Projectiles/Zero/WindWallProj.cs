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
    /// 风道风墙 — 滞留风墙弹幕。
    /// 风道技术储备库的"风墙"技术：
    /// - 在指定位置生成风墙，固定不动
    /// - 持续击退范围内的敌人
    /// - 对范围内的敌人造成持续伤害
    /// - 产生风之粒子效果
    /// - 持续一段时间后消失
    ///
    /// 行为组合：
    /// - 完全由 OnAI 控制（无移动行为）
    /// - 每帧检测范围内的敌人 + 击退 + 伤害
    /// - 风之粒子效果
    /// </summary>
    public class WindWallProj : BaseBullet
    {
        /// <summary>风墙持续时间（帧）</summary>
        private const int Duration = 120; // 2秒

        /// <summary>击退范围（像素）</summary>
        private const float PushRange = 100f;

        /// <summary>击退强度</summary>
        private const float PushStrength = 3f;

        /// <summary>伤害检测间隔（帧）</summary>
        private const int HitInterval = 10;

        /// <summary>伤害半径（像素）</summary>
        private const float HitRadius = 60f;

        /// <summary>计时器</summary>
        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            // 风墙不需要移动行为，完全由 OnAI 控制
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 80;
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

            // 透明度随持续时间变化（淡入淡出）
            float lifeRatio = _timer / (float)Duration;
            if (lifeRatio < 0.15f)
                Projectile.alpha = (int)(200 * (1f - lifeRatio / 0.15f));
            else if (lifeRatio > 0.7f)
                Projectile.alpha = (int)(200 * ((lifeRatio - 0.7f) / 0.3f));
            else
                Projectile.alpha = 40;

            // 击退范围内的敌人
            PushEnemies();

            // 间隔伤害
            if (_timer % HitInterval == 0)
            {
                DamageEnemiesInRange();
            }

            // 风之粒子效果
            SpawnWindParticles();

            // 光照
            Lighting.AddLight(Projectile.Center, 0.2f, 0.4f, 0.6f);
        }

        /// <summary>
        /// 击退范围内的敌人
        /// </summary>
        private void PushEnemies()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    Vector2 toNpc = npc.Center - Projectile.Center;
                    float dist = toNpc.Length();

                    if (dist <= PushRange && dist > 10f)
                    {
                        // 从中心向外击退
                        float pushForce = PushStrength * (1f - dist / PushRange);
                        Vector2 pushVelocity = toNpc.SafeNormalize(Vector2.Zero) * pushForce;
                        npc.velocity += pushVelocity;
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
                                Knockback = 5f, // 强击退
                                HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 生成风之粒子效果
        /// </summary>
        private void SpawnWindParticles()
        {
            // 旋转风环
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(20f, 45f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Cloud,
                    angle.ToRotationVector2() * Main.rand.NextFloat(0.5f, 2f),
                    50,
                    new Color(180, 220, 255, 120),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = true;
            }

            // 上升气流
            if (_timer % 3 == 0)
            {
                Vector2 upPos = Projectile.Center + Main.rand.NextVector2Circular(30f, 30f);
                Dust d = Dust.NewDustPerfect(
                    upPos,
                    DustID.Cloud,
                    new Vector2(
                        Main.rand.NextFloat(-0.5f, 0.5f),
                        -Main.rand.NextFloat(0.5f, 2f)
                    ),
                    50,
                    new Color(200, 230, 255, 100),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 消失时产生风之爆散
            for (int i = 0; i < 12; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Cloud,
                    vel,
                    0,
                    new Color(180, 220, 255, 180),
                    Main.rand.NextFloat(0.6f, 1.2f)
                );
                d.noGravity = true;
            }
        }
    }
}
