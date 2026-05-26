using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;
using Terraria.DataStructures;

namespace VerminLordMod.Content.Projectiles
{
    public class DianLuProj : BaseBullet
    {
        private const float FlySpeed = 9f;
        private const int MaxChains = 3;
        private const float ChainRange = 200f;

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(0.5f, 0.5f, 1.2f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(150, 150, 255),
                GlowLayers = 3,
                GlowBaseScale = 1.3f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.15f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(0.6f, 0.6f, 1.4f)
            });

            Behaviors.Add(new LightningTrailBehavior
            {
                EnableGhostTrail = true,
                GhostMaxPositions = 6,
                GhostAlpha = 0.5f,
                GhostColor = new Color(180, 180, 255, 200),
                AutoDraw = true,
                SuppressDefaultDraw = false
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.scale = 1.0f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 40;
            Projectile.alpha = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.knockBack = 2f;
        }

        protected override void OnSpawned(IEntitySource source)
        {
            Projectile.ai[0] = 0;
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int chainCount = (int)Projectile.ai[0];
            if (chainCount >= MaxChains)
                return;

            float closestDist = ChainRange;
            NPC closestNpc = null;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.whoAmI == target.whoAmI || !npc.CanBeChasedBy())
                    continue;
                float dist = Vector2.Distance(target.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestNpc = npc;
                }
            }

            if (closestNpc != null)
            {
                Vector2 dir = (closestNpc.Center - target.Center).SafeNormalize(Vector2.Zero);
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    dir * FlySpeed,
                    Projectile.type,
                    (int)(Projectile.damage * 0.8f),
                    Projectile.knockBack,
                    Projectile.owner,
                    chainCount + 1
                );
            }
        }

        protected override bool OnTileCollided(Vector2 oldVelocity) => true;
    }
}
