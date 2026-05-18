using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// BaoRanGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// BaoRanGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// BaoRanGu弹幕 — 道道
    /// </summary>



    /// <summary>




    /// BaoRanGu弹幕 — 火道




    /// </summary>




    public class BaoRanProj : BaseBullet
    {
        private const float FlySpeed = 7f;
        private const float BlastRadius = 150f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1f, 0.6f, 0.1f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.Torch,
                SpawnChance = 2,
                DustScale = 1.5f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 2f
            });

            Behaviors.Add(new OnKillAoEBehavior
            {
                Radius = BlastRadius,
                DamageMultiplier = 0.8f,
                Knockback = 8f,
                Buffs = new List<(int, int)>
                {
                    (BuffID.OnFire, 240)
                }
            });

            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 25,
                KillSpeed = 6f,
                KillSizeMultiplier = 1.2f,
                KillFragmentLife = 30,
                ExplodeOnTileCollide = true,
                TileCollideCount = 15,
                TileCollideSpeed = 4f,
                TileCollideSizeMultiplier = 1.0f,
                TileCollideFragmentLife = 25,
                ColorStart = new Color(255, 150, 30, 255),
                ColorEnd = new Color(200, 50, 0, 0)
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 20,
                        DustType = DustID.Torch,
                        Color = new Color(255, 100, 20),
                        ScaleMin = 1f,
                        ScaleMax = 2f,
                        SpeedMin = 2f,
                        SpeedMax = 7f,
                        SpreadRadius = 20f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
