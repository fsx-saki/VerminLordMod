using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class GuanGuProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 7f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.6f, 0.7f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(180, 190, 220),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(0.7f, 0.7f, 0.8f)
            });

            Behaviors.Add(new TrailBehavior
            {
                AutoDraw = true,
                SuppressDefaultDraw = false
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.alpha = 15;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 6f;
        }

        protected override void OnSpawned(Terraria.DataStructures.IEntitySource source)
        {
            var trailBehavior = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trailBehavior != null)
            {
                var trailTex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/GuanGuProj").Value;
                trailBehavior.TrailManager.NewTrail(trailTex,
                    color: new Color(180, 190, 220),
                    maxPositions: 14,
                    widthScale: 0.8f,
                    lengthScale: 1f,
                    alpha: 0.7f,
                    recordInterval: 2,
                    enableGlow: false);
            }
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<GuanGuSealBuff>(), 300);
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
