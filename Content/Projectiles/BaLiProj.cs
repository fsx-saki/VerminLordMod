using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 霸力蛊弹幕 — 力道
    /// </summary>

    public class BaLiProj : BaseBullet
    {
        private const float FlySpeed = 8f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.8f, 0.3f, 0.1f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.AmberBolt,
                SpawnChance = 2,
                DustScale = 1.0f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1.5f
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.AmberBolt,
                DustCount = 12,
                SpeedMin = 2f,
                SpeedMax = 5f,
                ScaleMin = 1.2f,
                ScaleMax = 2.5f,
                Color = Color.OrangeRed
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 15,
                        DustType = DustID.AmberBolt,
                        Color = new Color(200, 100, 30),
                        ScaleMin = 1f,
                        ScaleMax = 2f,
                        SpeedMin = 2f,
                        SpeedMax = 6f,
                        SpreadRadius = 12f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
