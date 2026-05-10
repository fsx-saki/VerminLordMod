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
    /// - 持续对范围内的敌人造成伤害
    /// - 附加减速 + 冻结效果
    /// - 产生冰霜粒子效果
    /// - 持续一段时间后消失
    ///
    /// 行为组合：
    /// - 完全由 OnAI 控制（无移动行为）
    /// - 每帧检测范围内的敌人 + 减速 + 伤害
    /// - 冰霜粒子效果
    /// </summary>
    public class IceFieldProj : BaseBullet
    {
        /// <summary>冰霜领域持续时间（帧）</summary>
        private const int Duration = 180; // 3秒

        /// <summary>伤害检测间隔（帧）</summary>
        private const int HitInterval = 15;

        /// <summary>伤害半径（像素）</summary>
        private const float HitRadius = 80f;

        /// <summary>减速范围（像素）</summary>
        private const float SlowRange = 100f;

        /// <summary>计时器</summary>
        private int _timer = 0;

        protected override void RegisterBehaviors()
        {
            // 冰霜领域不需要移动行为，完全由 OnAI 控制
        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 100;
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
                Projectile.alpha = 50;

            // 减速范围内的敌人
            SlowEnemies();

            // 间隔伤害
            if (_timer % HitInterval == 0)
            {
                DamageEnemiesInRange();
            }

            // 冰霜粒子效果
            SpawnIceParticles();

            // 光照
            Lighting.AddLight(Projectile.Center, 0.1f, 0.3f, 0.7f);
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
                        // 附加减速 + 冻结效果
                        npc.AddBuff(BuffID.Slow, 30);
                        npc.AddBuff(BuffID.Chilled, 30);
                        npc.AddBuff(BuffID.Frostburn, 30);
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
        /// 生成冰霜粒子效果
        /// </summary>
        private void SpawnIceParticles()
        {
            // 冰霜环
            for (int i = 0; i < 3; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float radius = Main.rand.NextFloat(20f, 50f);
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Ice,
                    angle.ToRotationVector2() * Main.rand.NextFloat(0.3f, 1.0f),
                    50,
                    new Color(180, 220, 255, 150),
                    Main.rand.NextFloat(0.4f, 0.8f)
                );
                d.noGravity = true;
            }

            // 冰晶下落
            if (_timer % 4 == 0)
            {
                Vector2 fallPos = Projectile.Center + Main.rand.NextVector2Circular(40f, 40f);
                Dust d = Dust.NewDustPerfect(
                    fallPos,
                    DustID.Ice,
                    new Vector2(
                        Main.rand.NextFloat(-0.3f, 0.3f),
                        Main.rand.NextFloat(0.5f, 1.5f)
                    ),
                    50,
                    new Color(200, 230, 255, 120),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = false;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 消失时产生冰晶爆散
            for (int i = 0; i < 15; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 5f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Ice,
                    vel,
                    0,
                    new Color(180, 220, 255, 200),
                    Main.rand.NextFloat(0.6f, 1.2f)
                );
                d.noGravity = true;
            }
        }
    }
}
