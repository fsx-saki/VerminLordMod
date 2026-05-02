using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    class DarkArrow : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new HomingBehavior(speed: 10f, trackingWeight: 1f / 11f)
            {
                Range = 8000f,
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 50;
            Projectile.timeLeft = 800;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
    }
}
