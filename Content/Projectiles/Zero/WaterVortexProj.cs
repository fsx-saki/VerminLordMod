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
    /// 水道漩涡 — 吸附敌人的漩涡弹幕。
    /// 水道技术储备库的"漩涡/吸附"技术：
    /// - 在指定位置生成漩涡，固定不动
    /// - 持续吸附范围内的敌人向中心拉拽
    /// - 对范围内的敌人造成持续伤害
    /// - 产生旋转水花粒子效果
    /// - 持续一段时间后消失
    ///
    /// 行为组合：
    /// - 完全由 OnAI 控制（无移动行为）
    /// - 每帧检测范围内的敌人 + 吸附 + 伤害
    /// - 旋转水花粒子效果
    /// </summary>
    public class WaterVortexProj : BaseBullet
    {
        /// <summary>漩涡持续时间（帧）</summary>
        private const int Duration = 240; // 4秒

        /// <summary>吸附范围（像素）</summary>
        private const float VortexRange = 160f;

        /// <summary>吸附力强度</summary>
        private const float PullStrength = 0.15f;

        /// <summary>伤害检测间隔（帧）</summary>
        private const int HitInterval = 10;

        /// <summary>伤害半径（像素）</summary>
        private const float HitRadius = 80f;

        /// <summary>计时器</summary>
        private int _timer = 0;

        /// <summary>旋转角度</summary>
        private float _rotationAngle = 0f;

        protected override void RegisterBehaviors()
        {
            // 漩涡不需要移动行为，完全由 OnAI 控制
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false; // 不碰撞
            Projectile.penetrate = -1;
            Projectile.timeLeft = Duration;
            Projectile.alpha = 50;
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
            _rotationAngle = Main.rand.NextFloat(MathHelper.TwoPi);
        }

        protected override void OnAI()
        {
            _timer++;
            _rotationAngle += 0.05f;

            // 1. 透明度随持续时间变化
            float lifeRatio = _timer / (float)Duration;
            if (lifeRatio < 0.15f)
                Projectile.alpha = (int)(255 * (1f - lifeRatio / 0.15f));
            else if (lifeRatio > 0.75f)
                Projectile.alpha = (int)(255 * ((lifeRatio - 0.75f) / 0.25f));
            else
                Projectile.alpha = 30;

            // 2. 吸附范围内的敌人
            PullEnemies();

            // 3. 间隔伤害
            if (_timer % HitInterval == 0)
            {
                DamageEnemiesInRange();
            }

            // 4. 旋转水花粒子效果
            SpawnVortexParticles();

            // 5. 光照
            Lighting.AddLight(Projectile.Center, 0.1f, 0.3f, 0.8f);
        }

        /// <summary>
        /// 吸附范围内的敌人向漩涡中心拉拽
        /// </summary>
        private void PullEnemies()
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    Vector2 toCenter = Projectile.Center - npc.Center;
                    float dist = toCenter.Length();

                    if (dist <= VortexRange && dist > 10f)
                    {
                        // 吸附力随距离变化（越近越强）
                        float pullForce = PullStrength * (1f - dist / VortexRange);
                        Vector2 pullVelocity = toCenter.SafeNormalize(Vector2.Zero) * pullForce;

                        // 添加切向速度（旋转效果）
                        Vector2 tangent = new Vector2(-toCenter.Y, toCenter.X).SafeNormalize(Vector2.Zero);
                        float tangentForce = pullForce * 0.5f;
                        pullVelocity += tangent * tangentForce;

                        npc.velocity += pullVelocity;

                        // 限制 NPC 速度，防止被拉飞
                        if (npc.velocity.Length() > 8f)
                            npc.velocity = npc.velocity.SafeNormalize(Vector2.Zero) * 8f;
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
                        int damage = Projectile.damage;
                        Player owner = Main.player[Projectile.owner];
                        if (owner != null && owner.active)
                        {
                            bool crit = Main.rand.Next(100) < Projectile.CritChance;
                            npc.StrikeNPC(new NPC.HitInfo
                            {
                                Damage = damage,
                                Knockback = 0f, // 漩涡不击退
                                HitDirection = npc.Center.X > Projectile.Center.X ? 1 : -1,
                                Crit = crit
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 生成旋转水花粒子效果
        /// </summary>
        private void SpawnVortexParticles()
        {
            // 旋转水花环
            for (int i = 0; i < 4; i++)
            {
                float angle = _rotationAngle + MathHelper.TwoPi * i / 4;
                float radius = 30f + (float)System.Math.Sin(_timer * 0.03f + i) * 15f;
                Vector2 pos = Projectile.Center + angle.ToRotationVector2() * radius;

                Dust d = Dust.NewDustPerfect(
                    pos,
                    DustID.Water,
                    angle.ToRotationVector2() * 0.5f,
                    0,
                    new Color(60, 180, 255, 150),
                    Main.rand.NextFloat(0.6f, 1.0f)
                );
                d.noGravity = true;
            }

            // 中心水花
            if (_timer % 3 == 0)
            {
                Vector2 centerPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Dust d = Dust.NewDustPerfect(
                    centerPos,
                    DustID.Water,
                    Main.rand.NextVector2Circular(1f, 1f),
                    0,
                    new Color(100, 220, 255, 200),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
            }

            // 气泡（向上飘）
            if (_timer % 5 == 0)
            {
                Vector2 bubblePos = Projectile.Center + Main.rand.NextVector2Circular(40f, 40f);
                Dust d = Dust.NewDustPerfect(
                    bubblePos,
                    DustID.Water,
                    new Vector2(
                        Main.rand.NextFloat(-0.3f, 0.3f),
                        -Main.rand.NextFloat(0.5f, 1.5f)
                    ),
                    50,
                    new Color(150, 230, 255, 100),
                    Main.rand.NextFloat(0.3f, 0.6f)
                );
                d.noGravity = true;
            }
        }

        protected override void OnKilled(int timeLeft)
        {
            // 消失时产生水花爆裂
            for (int i = 0; i < 15; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2f, 6f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                    DustID.Water,
                    vel,
                    0,
                    new Color(80, 200, 255, 200),
                    Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
            }
        }
    }
}
