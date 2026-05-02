using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    class DingShenProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
            Behaviors.Add(new TrailBehavior { AutoDraw = true });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.alpha = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            var trail = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trail != null)
            {
                trail.TrailManager.AddGhostTrail(
                    trailTex: TextureAssets.Projectile[Projectile.type].Value,
                    color: new Color(100, 180, 255),
                    maxPositions: 16,
                    widthScale: 0.5f,
                    lengthScale: 1.0f,
                    alpha: 0.5f,
                    recordInterval: 2);
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DingShenbuff>(), 200);
        }
    }
}
