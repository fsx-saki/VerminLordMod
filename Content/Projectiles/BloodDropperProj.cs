using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// BloodDropperGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// BloodDropperGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// BloodDropperGu弹幕 — 道道
    /// </summary>



    /// <summary>




    /// BloodDropperGu弹幕 — 道道




    /// </summary>




    public class BloodDropperProj : BaseBullet
    {
        private const float OutwardSpeed = 14f;
        private const float ReturnSpeed = 18f;
        private const int OutwardFrames = 30;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new ChargeProjectileBehavior
            {
                MaxChargeTime = 300,
                ChargeDistance = 0f,
                StartScale = 0.4f,
                EndScale = 1.2f,
                StartAlpha = 100,
                EndAlpha = 0,
                DamageMultiplier = 0f,
                ChargeRotationSpeed = 0.3f,
                FireSpeed = OutwardSpeed,
                ChargePositionOffset = new Vector2(0, -40f),
                OnChargeParticle = (proj, progress) =>
                {
                    Player owner = Main.player[proj.owner];
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = Main.rand.NextFloat(15f, 30f);
                        Dust d = Dust.NewDustPerfect(
                            owner.Center + new Vector2(0, -40f) + angle.ToRotationVector2() * dist,
                            DustID.Blood,
                            Vector2.Zero,
                            100,
                            Color.DarkRed,
                            Main.rand.NextFloat(0.8f, 1.5f)
                        );
                        d.noGravity = true;
                    }
                },
                OnFire = (proj, progress) =>
                {
                    proj.velocity = (Main.MouseWorld - Main.player[proj.owner].Center).SafeNormalize(Vector2.UnitY) * OutwardSpeed;
                }
            });

            Behaviors.Add(new BoomerangBehavior(OutwardSpeed, ReturnSpeed, OutwardFrames)
            {
                SpinSpeed = 0.5f,
                AutoRotate = false
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.7f, 0.1f, 0.1f)));

            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 40,
                FragmentLife = 28,
                SizeMultiplier = 0.7f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 100f,
                SpeedLifeExponent = 0.3f,
                MinFragmentLife = 8,
                ColorStart = new Color(255, 60, 40, 255),
                ColorEnd = new Color(180, 0, 0, 0),
                Buoyancy = 0.02f,
                AirResistance = 0.96f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.15f,
                SplashAngle = 0.4f,
                RandomSpread = 0.5f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 80, 80),
                GlowLayers = 2,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)> { (BuffID.Bleeding, 180) }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Blood,
                DustCount = 15,
                SpeedMin = 1f,
                SpeedMax = 4f,
                ScaleMin = 1f,
                ScaleMax = 2f,
                Color = Color.Red
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 20,
                        DustType = DustID.Blood,
                        Color = Color.Crimson,
                        ScaleMin = 1f,
                        ScaleMax = 2.5f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 10f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}