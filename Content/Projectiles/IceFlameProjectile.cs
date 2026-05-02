using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    class IceFlameProjectile : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
            Behaviors.Add(new TrailBehavior { AutoDraw = true });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            var trail = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trail != null)
            {
                Texture2D myTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/IceFlameProjectile").Value;
                trail.TrailManager.AddGhostTrail(myTex,
                    color: new Color(255, 255, 200),
                    maxPositions: 50,
                    widthScale: 0.5f,
                    lengthScale: 1f,
                    alpha: 0.8f,
                    recordInterval: 1);
            }
        }
    }
}
