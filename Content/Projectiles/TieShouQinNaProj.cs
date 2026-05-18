using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// TieShouQinNaGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// TieShouQinNaGu弹幕 — 道道
    /// </summary>    /// <summary>
    /// TieShouQinNaGu弹幕 — 道道
    /// </summary>



    /// <summary>




    /// TieShouQinNaGu弹幕 — 道道




    /// </summary>




    public class TieShouQinNaProj : BaseBullet
    {
        private const float FlySpeed = 10f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: FlySpeed, trackingWeight: 1f / 18f)
            {
                Range = 500f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.GoldFlame,
                SpawnChance = 2,
                DustScale = 0.8f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1f
            });

            Behaviors.Add(new DebuffOnHitBehavior
            {
                Buffs = new List<(int, int)>
                {
                    (BuffID.Slow, 180)
                }
            });

            Behaviors.Add(new DustOnHitBehavior
            {
                DustType = DustID.GoldFlame,
                DustCount = 10,
                SpeedMin = 1f,
                SpeedMax = 3f,
                ScaleMin = 1f,
                ScaleMax = 2f,
                Color = Color.Gold
            });

            Behaviors.Add(new KillDustBurstBehavior
            {
                Layers = new List<KillDustBurstBehavior.DustBurstLayer>
                {
                    new()
                    {
                        Count = 12,
                        DustType = DustID.GoldFlame,
                        Color = new Color(200, 170, 50),
                        ScaleMin = 0.8f,
                        ScaleMax = 1.5f,
                        SpeedMin = 2f,
                        SpeedMax = 5f,
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
