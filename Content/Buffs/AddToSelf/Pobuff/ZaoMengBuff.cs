using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using VerminLordMod.Content.Projectiles;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ZaoMengBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<ZaoMengPlayer>().HasZaoMeng = true;
        }
    }

    public class ZaoMengPlayer : ModPlayer
    {
        public bool HasZaoMeng { get; set; }
        private const float DodgeChance = 0.08f;
        private const float CloneChance = 0.10f;
        private const float CloneDamageMultiplier = 0.30f;
        private int _cloneCooldown;

        public override void ResetEffects()
        {
            HasZaoMeng = false;
        }

        public override void PostUpdate()
        {
            if (!HasZaoMeng)
                return;

            Player.GetDamage(DamageClass.Generic) += 0.15f;
            Player.GetCritChance(DamageClass.Generic) += 10f;

            if (_cloneCooldown > 0)
                _cloneCooldown--;

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 50f);
                var pos = Player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, ModContent.DustType<DreamDust>());
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (HasZaoMeng && Main.rand.NextFloat() < DodgeChance)
            {
                Player.NinjaDodge();
                CombatText.NewText(Player.Hitbox, new Color(180, 120, 255), "梦遁!", true);
                return true;
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasZaoMeng || _cloneCooldown > 0)
                return;

            if (Main.rand.NextFloat() < CloneChance)
            {
                _cloneCooldown = 30;
                Vector2 spawnPos = Player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -20f);
                Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * 8f;
                int cloneDamage = (int)(damageDone * CloneDamageMultiplier);
                if (cloneDamage < 1) cloneDamage = 1;
                Projectile.NewProjectile(Player.GetSource_OnHit(target), spawnPos, vel, ModContent.ProjectileType<ZaoMengCloneProj>(), cloneDamage, 2f, Player.whoAmI);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasZaoMeng || _cloneCooldown > 0)
                return;

            if (Main.rand.NextFloat() < CloneChance)
            {
                _cloneCooldown = 30;
                Vector2 spawnPos = Player.Center + new Vector2(Main.rand.NextFloat(-30f, 30f), -20f);
                Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * 8f;
                int cloneDamage = (int)(damageDone * CloneDamageMultiplier);
                if (cloneDamage < 1) cloneDamage = 1;
                Projectile.NewProjectile(Player.GetSource_OnHit(target), spawnPos, vel, ModContent.ProjectileType<ZaoMengCloneProj>(), cloneDamage, 2f, Player.whoAmI);
            }
        }
    }
}
