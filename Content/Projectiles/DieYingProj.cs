using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// DieYingGu弹幕 — 影道
    /// </summary>
    public class DieYingProj : BaseBullet
    {
        private const float FlySpeed = 14f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.3f, 0.1f, 0.4f)
            });

            Behaviors.Add(new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.Shadowflame,
                SpawnChance = 3,
                DustScale = 0.6f,
                VelocityMultiplier = 0.05f,
                NoGravity = true,
                RandomSpeed = 1f
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.Shadowflame,
                DustCount = 8,
                SpeedMin = 1f,
                SpeedMax = 3f,
                ScaleMin = 0.8f,
                ScaleMax = 1.5f,
                Color = Color.DarkSlateGray
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 10,
                        DustType = DustID.Shadowflame,
                        Color = new Color(80, 40, 100),
                        ScaleMin = 0.6f,
                        ScaleMax = 1.2f,
                        SpeedMin = 2f,
                        SpeedMax = 5f,
                        SpreadRadius = 8f
                    }
                }
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
