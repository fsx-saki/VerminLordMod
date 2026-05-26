using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using Terraria.GameContent;

namespace VerminLordMod.Content.Projectiles
{
    public class ZhengYiProj : BaseBullet
    {
        private const float FlySpeed = 9f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.5f, 1.3f, 0.5f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 220, 80),
                GlowLayers = 3,
                GlowBaseScale = 1.2f,
                GlowScaleIncrement = 0.4f,
                GlowBaseAlpha = 0.5f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.3f,
                EnableLight = true,
                LightColor = new Vector3(1.5f, 1.3f, 0.5f)
            });

            Behaviors.Add(new DustTrailBehavior
            {
                DustType = DustID.GoldFlame,
                SpawnChance = 3,
                DustScale = 1.0f,
                VelocityMultiplier = 0.1f,
                NoGravity = true,
                RandomSpeed = 1.5f
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1f;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = true;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 80;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            bool hasDebuff = false;
            for (int i = 0; i < NPC.maxBuffs; i++)
            {
                if (target.buffType[i] > 0 && target.buffTime[i] > 0 && Main.debuff[target.buffType[i]])
                {
                    hasDebuff = true;
                    break;
                }
            }

            if (hasDebuff)
            {
                int bonusDamage = (int)(hit.Damage * 0.25f);
                if (bonusDamage > 0)
                {
                    target.SimpleStrikeNPC(bonusDamage, hit.HitDirection, crit: false, knockBack: 0f);
                    CombatText.NewText(target.Hitbox, new Color(255, 215, 0), "正义裁决!", true);
                }
            }

            if (Main.rand.NextFloat() < 0.20f)
            {
                target.AddBuff(BuffID.Ichor, 180);
            }

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.GoldFlame);
                d.noGravity = true;
                d.velocity = Main.rand.NextVector2Circular(3f, 3f);
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
