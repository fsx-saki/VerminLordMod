using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Projectiles
{
    class SoulShockWave : BaseBullet
    {
        private Texture2D tex;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 0f) { AutoRotate = true, RotationOffset = MathHelper.PiOver2 });
            Behaviors.Add(new TrailBehavior { AutoDraw = true });
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.5f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 60;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;

            tex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/SoulShockWave").Value;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            var trail = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trail != null)
            {
                Texture2D trailTex = ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/MoonlightProjTail").Value;
                trail.TrailManager.AddGhostTrail(trailTex,
                    color: new Color(150, 220, 255),
                    maxPositions: 16,
                    widthScale: 0.4f,
                    lengthScale: 2f,
                    alpha: 0.8f,
                    recordInterval: 2);
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<SoulOscillationbuff>(), 120);
        }
    }
}
