using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 土道基础弹幕 — 泥浆重力。
    ///
    /// 设计哲学：
    /// 土道的本质是"重力 + 粘滞 + 厚重"。弹幕受强重力影响做抛物线运动，
    /// 命中后产生径向泥浆爆散（SplashBehavior Radial 模式），
    /// 视觉上以泥系拖尾（泥块 + 裂纹 + 泥滴）和厚重感模拟泥球的砸击力。
    ///
    /// 运动方式：
    /// - 抛物线运动（GravityBehavior）
    /// - 命中后径向泥浆爆散
    ///
    /// 视觉效果：
    /// - 泥系拖尾：泥块 + 裂纹 + 泥滴（MudTrailBehavior）
    /// - 命中时径向泥浆爆散（SplashBehavior Radial 模式）
    ///
    /// 行为组合：
    /// - AimBehavior: 初始速度
    /// - GravityBehavior: 强重力抛物线
    /// - MudTrailBehavior: 泥系拖尾（泥块 + 裂纹 + 泥滴）
    /// - SplashBehavior(Radial): 命中时径向泥浆爆散
    /// </summary>
    public class MudBaseProj : BaseBullet
    {
        private const float FlySpeed = 8f;
        private const int MaxLife = 240;

        protected override void RegisterBehaviors()
        {
            // 1. 初始速度
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.2f, 0.15f, 0.05f)
            });

            // 2. 强重力抛物线
            Behaviors.Add(new GravityBehavior(acceleration: 0.25f, maxFallSpeed: 14f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            // 3. 泥系拖尾
            Behaviors.Add(new MudTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(120, 80, 40, 140),
                MudClodColor = new Color(100, 70, 30, 200),
                GroundCrackColor = new Color(80, 55, 25, 180),
                SludgeDripColor = new Color(110, 75, 35, 160),
            });

            // 4. 命中时径向泥浆爆散
            Behaviors.Add(new SplashBehavior(SplashMode.Radial)
            {
                Count = 10,
                SpeedMin = 1f,
                SpeedMax = 4f,
                SpreadRadius = 5f,
                SpawnExtraDust = true,
                ExtraDustCount = 12,
                DustType = DustID.Mud,
                DustColorStart = new Color(100, 70, 30, 200),
                DustColorEnd = new Color(60, 40, 15, 0),
                DustScaleMin = 0.3f,
                DustScaleMax = 0.7f,
                DustSpeedMin = 0.5f,
                DustSpeedMax = 2.5f,
                DustNoGravity = false,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = MaxLife;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Slow, 180);
            target.AddBuff(BuffID.OgreSpit, 120);
        }

        protected override void OnKilled(int timeLeft)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(0.5f, 2f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Mud,
                    vel,
                    0,
                    new Color(100, 70, 30, 150),
                    Main.rand.NextFloat(0.4f, 0.7f)
                );
                d.noGravity = false;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}