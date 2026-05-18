using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.SmoothMovement.Interpolators;
using VerminLordMod.Content.SmoothMovement.Orbiters;
using VerminLordMod.Content.SmoothMovement.Trackers;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// FlyingBoneShieldGu弹幕 — 骨道
    /// </summary>
    public class FlyingBoneShieldProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            float baseAngle = Projectile.ai[1];
            Behaviors.Add(new OrbitChaseReturnBehavior(
                new ChaoticOrbiter(
                    initialAngle: baseAngle,
                    angularSpeed: 0.03f,
                    baseRadius: 65f,
                    radiusWaveRange: 15f,
                    ellipseCompression: 0.7f),
                new SpiralApproachTracker(
                    speed: 14f,
                    angularFrequency: 0.06f,
                    baseAmplitude: 40f,
                    spiralRampDuration: 40f),
                new LerpSmoothInterpolator(transitionSpeed: 0.12f))
            {
                ChaseRange = 280,
                ReturnDistance = 500f,
                HitDistThreshold = 35f,
                ReturnRadiusMargin = 25f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.2f, 0.2f, 0.1f)));

            Behaviors.Add(new BoneTrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(240, 230, 200),
                GlowLayers = 2,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.2f,
                GlowBaseAlpha = 0.3f,
                GlowAlphaDecay = 0.08f,
                GlowAlphaMultiplier = 0.15f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)> { (BuffID.BrokenArmor, 120) }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Bone,
                DustCount = 8,
                SpeedMin = 0.5f,
                SpeedMax = 3f,
                ScaleMin = 0.5f,
                ScaleMax = 1f,
                Color = Color.White
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 10,
                        DustType = DustID.Bone,
                        Color = Color.White,
                        ScaleMin = 0.5f,
                        ScaleMax = 1.2f,
                        SpeedMin = 1f,
                        SpeedMax = 4f,
                        SpreadRadius = 10f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 600;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}