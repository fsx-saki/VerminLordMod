using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class FlyingSoulProj : BaseBullet
    {
        private const float FlySpeed = 6f;
        private const float TrackWeight = 1f / 22f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: TrackWeight)
            {
                Range = 900f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.1f, 0.15f, 0.5f)));

            Behaviors.Add(new SoulTrailBehavior
            {
                SuppressDefaultDraw = true,
                GhostColor = new Color(100, 150, 220, 140),
                FlameColor = new Color(120, 180, 240, 200),
                ChainColor = new Color(80, 140, 220, 180),
                WispColor = new Color(150, 200, 250, 220)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(160, 200, 240),
                GlowLayers = 2,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.25f,
                GlowBaseAlpha = 0.4f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.25f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)> { (BuffID.Confused, 150) }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.SpectreStaff,
                DustCount = 8,
                SpeedMin = 0.5f,
                SpeedMax = 2.5f,
                ScaleMin = 0.4f,
                ScaleMax = 0.7f,
                Color = new Color(120, 160, 230)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 10,
                        DustType = DustID.SpectreStaff,
                        Color = new Color(120, 160, 230, 130),
                        ScaleMin = 0.3f,
                        ScaleMax = 0.6f,
                        SpeedMin = 0.5f,
                        SpeedMax = 2f,
                        SpreadRadius = 10f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.alpha = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}