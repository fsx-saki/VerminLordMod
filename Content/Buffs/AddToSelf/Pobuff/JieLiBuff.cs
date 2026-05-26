using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JieLiBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.GetModPlayer<JieLiPlayer>().HasJieLi = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PurpleTorch);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class JieLiPlayer : ModPlayer
    {
        public bool HasJieLi { get; set; }
        private const float BacklashChance = 0.05f;
        private const int BacklashDamage = 10;

        public override void ResetEffects()
        {
            HasJieLi = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasJieLi)
                return;

            if (Main.rand.NextFloat() < BacklashChance)
            {
                Player.Hurt(PlayerDeathReason.LegacyDefault(), BacklashDamage, 0, cooldownCounter: 5);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasJieLi)
                return;

            if (Main.rand.NextFloat() < BacklashChance)
            {
                Player.Hurt(PlayerDeathReason.LegacyDefault(), BacklashDamage, 0, cooldownCounter: 5);
            }
        }
    }
}
