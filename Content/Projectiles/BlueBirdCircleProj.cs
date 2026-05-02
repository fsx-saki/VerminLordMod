using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Buffs.AddToEnemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// 蓝鸟冰霜法阵 — 在敌人位置生成，高速旋转并发蓝光。
    /// 使用 CircleArrayDrawer 绘制双重同心圆法阵。
    /// 定期从四面八方召唤冰碎片汇聚向敌人，持续冻结目标。
    /// </summary>
    public class BlueBirdCircleProj : ModProjectile
    {
        // ===== 配置常量 =====
        private const int Lifetime = 240;           // 法阵持续 4 秒
        private const int ShardSpawnInterval = 12;  // 每 12 帧生成一个碎片

        // ===== 法阵绘制器 =====
        private CircleArrayDrawer _circleDrawer;

        // ===== 运行时状态 =====
        private int _shardTimer;
        private NPC _target;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 0;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.aiStyle = -1;
        }

        public override void OnSpawn(IEntitySource source)
        {
            // 初始化法阵绘制器（固定模式，chargeProgress = -1）
            _circleDrawer = new CircleArrayDrawer(
                texPath: "VerminLordMod/Content/Projectiles/BlueBirdCircleProj",
                outerRadius: 100f,
                innerRadius: 65f,
                colorOuter: new Color(100, 180, 255, 150),
                colorInner: new Color(150, 220, 255, 180),
                rotationSpeed1: 0.12f,
                rotationSpeed2: -0.09f)
            {
                // 双层光晕
                EnableDualGlow = true,
                GlowColor1 = new Color(80, 180, 255, 120),
                GlowColor2 = new Color(50, 120, 255, 40),
                GlowScale1 = 0.35f,
                GlowScale2 = 2.0f,

                // 光照
                EnableLighting = true,
                LightColor = new Vector3(0.2f, 0.4f, 0.8f),

                // 粒子环绕
                EnableParticleRing = true,
                ParticleDustType = DustID.Ice,
                ParticleChance = 2,
                ParticleScale = 1.0f,
                ParticleAlpha = 50,
                ParticleVelocityMultiplier = 0.5f,
            };
            _circleDrawer.Init(
                Main.rand.NextFloat(MathHelper.TwoPi),
                Main.rand.NextFloat(MathHelper.TwoPi));

            _shardTimer = 0;

            // 锁定目标
            int npcIndex = (int)Projectile.ai[0];
            if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
            {
                _target = Main.npc[npcIndex];
                if (_target != null && _target.active)
                {
                    Projectile.Center = _target.Center;
                }
            }
        }

        public override void AI()
        {
            // 1. 法阵旋转
            _circleDrawer.Update();

            // 2. 光照
            _circleDrawer.UpdateLighting(Projectile.Center);

            // 3. 粒子环绕
            _circleDrawer.UpdateParticleRing(Projectile.Center);

            // 4. 跟随目标
            if (_target != null && _target.active)
            {
                Projectile.Center = _target.Center;
            }
            else
            {
                Projectile.Kill();
                return;
            }

            // 5. 持续给目标叠加冻结 buff
            if (Main.time % 20 == 0)
            {
                _target.AddBuff(ModContent.BuffType<BlueBirdbuff>(), Lifetime + 60);
            }

            // 6. 定期生成冰碎片
            _shardTimer++;
            if (_shardTimer >= ShardSpawnInterval)
            {
                _shardTimer = 0;
                SpawnIceShard();
            }
        }

        private void SpawnIceShard()
        {
            Vector2 center = Projectile.Center;
            // 使用 Projectile.NewProjectileDirect 确保在 OnSpawn 之前设置 ai 值
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float distance = Main.rand.NextFloat(100f, 250f);
            Vector2 spawnPos = center + angle.ToRotationVector2() * distance;
            Vector2 vel = (center - spawnPos).SafeNormalize(Vector2.Zero) * 7f;

            int projIndex = Projectile.NewProjectile(
                Projectile.GetSource_FromThis(), spawnPos, vel,
                ModContent.ProjectileType<IceShardProj>(),
                0, 0f, Projectile.owner,
                ai0: center.X, ai1: center.Y);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // 使用 CircleArrayDrawer 绘制法阵（固定模式）
            _circleDrawer.Draw(Main.spriteBatch, Projectile.Center, chargeProgress: -1f);
            return false;
        }
    }
}
