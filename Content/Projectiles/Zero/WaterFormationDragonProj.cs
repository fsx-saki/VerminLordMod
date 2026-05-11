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
    /// 水道法阵龙弹 — 法阵释放的粒子体水球，以约束转向追踪鼠标。
    ///
    /// 双绞线中的单颗水球，使用 ConstrainedSteerBehavior 平滑追踪鼠标，
    /// 形成"水龙"蜿蜒追逐指针的效果。
    ///
    /// 行为组合：
    /// - ParticleBodyBehavior: 粒子水球本体
    /// - WaterTrailBehavior: 水系拖尾
    /// - ConstrainedSteerBehavior: 约束转向追踪鼠标
    /// - SuppressDrawBehavior: 阻止默认贴图绘制
    /// </summary>
    public class WaterFormationDragonProj : BaseBullet
    {
        private const float InitialSpeed = 6f;
        private const float AccelMagnitude = 0.25f;
        private const float MaxSpeed = 10f;
        private const int Lifetime = 100;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ParticleBodyBehavior(particleCount: 10, bodyRadius: 6f)
            {
                ParticleSize = 0.7f,
                ColorStart = new Color(30, 100, 210, 220),
                ColorEnd = new Color(30, 100, 210, 220),
                SwirlSpeed = 0.08f,
                ReturnForce = 0.8f,
                JitterStrength = 0.4f,
                ShrinkOverLife = true,
                StretchOnMove = true,
                StretchFactor = 0.4f,
                EnableLight = false,
            });

            Behaviors.Add(new WaterTrailBehavior
            {
                MaxFragments = 12,
                ParticleLife = 6,
                SizeMultiplier = 0.3f,
                SpawnChance = 0.5f,
                SplashSpeed = 0.15f,
                SplashAngle = 0.12f,
                InertiaFactor = 0.03f,
                RandomSpread = 0.5f,
                Gravity = 0.12f,
                AirResistance = 0.97f,
                BubbleChance = 0.35f,
                BubbleSizeMultiplier = 1.3f,
                ColorStart = new Color(30, 100, 210, 200),
                ColorEnd = new Color(30, 100, 210, 0),
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new ConstrainedSteerBehavior(AccelMagnitude, MaxSpeed)
            {
                TrackMouse = true,
                ConeHalfAngle = MathHelper.Pi / 6f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = false,
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.04f, 0.12f, 0.25f)));

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new System.Collections.Generic.List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new KillDustBurstBehavior.DustBurstLayer
                    {
                        Count = 3,
                        DustType = DustID.Water,
                        Color = new Color(50, 150, 220, 120),
                        ScaleMin = 0.3f,
                        ScaleMax = 0.6f,
                        NoGravity = true,
                        SpreadRadius = 3f,
                        UseCircularVelocity = true,
                        SpeedMin = 0f,
                        SpeedMax = 2f,
                    },
                }
            });

            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            Projectile.width = 6;
            Projectile.height = 6;
            Projectile.scale = 0.7f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }
    }
}