using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{    /// <summary>
    /// 光虹蛊弹幕 — 道道
    /// </summary>

    public class GuangHongProj : BaseBullet
    {
        private const float FlySpeed = 16f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1f, 1f, 0.8f)
            });

            Behaviors.Add(new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.AncientLight,
                SpawnChance = 2,
                DustScale = 0.6f,
                VelocityMultiplier = 0.05f,
                NoGravity = true,
                RandomSpeed = 1f
            });

            Behaviors.Add(new DustKillBehavior
            {
                DustType = DustID.AncientLight,
                DustCount = 8,
                DustSpeed = 3f,
                DustScale = 1f,
                NoGravity = true
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
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
