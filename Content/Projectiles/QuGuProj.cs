using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using Terraria.GameContent;

namespace VerminLordMod.Content.Projectiles
{
    public class QuGuProj : BaseBullet
    {
        private int _buffsRemoved = 0;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: 9f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.9f, 0.9f, 1.0f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(220, 220, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 1.0f, 1.2f)
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
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 4f;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            var trailBehavior = Behaviors.Find(b => b is TrailBehavior) as TrailBehavior;
            if (trailBehavior != null)
            {
                var trailTex = ModContent.Request<Texture2D>(
                    "VerminLordMod/Content/Projectiles/QuGuProj").Value;
                trailBehavior.TrailManager.NewTrail(trailTex,
                    color: new Color(220, 220, 255),
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
            _buffsRemoved = 0;

            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                if (target.buffType[i] != 0 && !Main.debuff[target.buffType[i]])
                {
                    target.buffType[i] = 0;
                    target.buffTime[i] = 0;
                    _buffsRemoved++;
                }
            }

            if (_buffsRemoved > 0)
            {
                int bonusDamage = (int)(damageDone * 0.5f * _buffsRemoved);
                target.StrikeNPC(new NPC.HitInfo
                {
                    Damage = bonusDamage,
                    HitDirection = hit.HitDirection,
                    Knockback = 0f
                });

                for (int i = 0; i < _buffsRemoved * 3; i++)
                {
                    var d = Dust.NewDustDirect(target.position, target.width, target.height,
                        DustID.PurificationPowder);
                    d.velocity *= 0.5f;
                    d.noGravity = true;
                    d.scale = 1.0f;
                }

                if (Projectile.owner != -1 && Projectile.owner < Main.maxPlayers)
                {
                    Player owner = Main.player[Projectile.owner];
                    Text.ShowTextGreen(owner, $"去蛊消除了{_buffsRemoved}个增益！额外造成{bonusDamage}伤害");
                }
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
