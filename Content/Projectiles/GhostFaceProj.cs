using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class GhostFaceProj : BaseBullet
    {
        private const float FlySpeed = 7f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new GlowLightBehavior(new Vector3(0.15f, 0.2f, 0.6f)));

            Behaviors.Add(new SoulTrailBehavior
            {
                SuppressDefaultDraw = true,
                EnableGhostTrail = true,
                GhostColor = new Color(140, 160, 220, 140),
                FlameColor = new Color(120, 140, 210, 200),
                ChainColor = new Color(90, 120, 200, 180),
                WispColor = new Color(160, 190, 240, 220)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(180, 200, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.3f,
                GlowBaseAlpha = 0.45f,
                GlowAlphaDecay = 0.13f,
                GlowAlphaMultiplier = 0.28f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)> { (BuffID.Confused, 240) }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.SpectreStaff,
                DustCount = 10,
                SpeedMin = 0.5f,
                SpeedMax = 3f,
                ScaleMin = 0.5f,
                ScaleMax = 1f,
                Color = new Color(140, 170, 240)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 12,
                        DustType = DustID.SpectreStaff,
                        Color = new Color(130, 170, 250, 150),
                        ScaleMin = 0.4f,
                        ScaleMax = 0.8f,
                        SpeedMin = 0.5f,
                        SpeedMax = 2.5f,
                        SpreadRadius = 10f
                    }
                }
            });

            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 16,
                KillSpeed = 3f,
                KillSizeMultiplier = 0.8f,
                KillFragmentLife = 30,
                ExplodeOnTileCollide = true,
                TileCollideCount = 10,
                TileCollideSpeed = 2f,
                TileCollideSizeMultiplier = 0.6f,
                TileCollideFragmentLife = 25,
                ColorStart = new Color(160, 190, 240, 200),
                ColorEnd = new Color(40, 70, 160, 0)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 240;
            Projectile.alpha = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => false;
    }
}