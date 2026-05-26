using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles
{
    public class DaoKeDaoProj : BaseBullet
    {
        private const float FlySpeed = 8f;

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.scale = 1.2f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 150;
            Projectile.alpha = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void RegisterBehaviors()
        {
            Behaviors.Add(new AimBehavior(speed: FlySpeed)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.95f, 0.7f)
            });

            Behaviors.Add(new GlowDrawBehavior
            {
                GlowColor = new Color(255, 245, 200),
                GlowLayers = 4,
                GlowBaseScale = 1.4f,
                GlowScaleIncrement = 0.5f,
                GlowBaseAlpha = 0.6f,
                GlowAlphaDecay = 0.12f,
                GlowAlphaMultiplier = 0.35f,
                EnableLight = true,
                LightColor = new Vector3(1.0f, 0.95f, 0.7f)
            });
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            var info = target.GetGlobalNPC<DaoKeDaoNPC>();
            if (info.DamageReductionStacks < 5)
            {
                info.DamageReductionStacks++;
            }
        }
    }

    public class DaoKeDaoNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public int DamageReductionStacks { get; set; }

        public override void ResetEffects(NPC npc)
        {
        }

        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (DamageReductionStacks > 0)
            {
                modifiers.FinalDamage *= 1f - 0.1f * DamageReductionStacks;
            }
        }

        public override void SaveData(NPC npc, Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["DamageReductionStacks"] = DamageReductionStacks;
        }

        public override void LoadData(NPC npc, Terraria.ModLoader.IO.TagCompound tag)
        {
            DamageReductionStacks = tag.GetInt("DamageReductionStacks");
        }
    }
}
