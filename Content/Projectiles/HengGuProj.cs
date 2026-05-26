using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using Terraria.ID;

namespace VerminLordMod.Content.Projectiles
{
    public class HengGuProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 6f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.9f, 0.5f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 220, 80),
                GlowLayers = 5,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.1f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(1.2f, 1.0f, 0.5f)
            });

            Behaviors.Add(new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 99;
            Projectile.timeLeft = 300;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 3f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            var trailBehavior = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trailBehavior != null)
            {
                var trailTex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/HengGuProj").Value;
                trailBehavior.TrailManager.NewTrail(trailTex,
                    color: new Color(255, 220, 80),
                    maxPositions: 20,
                    widthScale: 0.9f,
                    lengthScale: 1f,
                    alpha: 0.8f,
                    recordInterval: 2,
                    enableGlow: false);
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.defense > 0)
            {
                target.defense -= 2;
                if (target.defense < 0)
                    target.defense = 0;
            }

            for (int i = 0; i < 5; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height,
                    Terraria.ID.DustID.GoldFlame);
                d.velocity *= 0.5f;
                d.noGravity = true;
                d.scale = 1.2f;
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
