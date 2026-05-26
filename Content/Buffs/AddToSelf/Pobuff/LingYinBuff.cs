using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class LingYinBuff : ModBuff
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
            player.invis = true;
            player.GetModPlayer<LingYinPlayer>().HasLingYin = true;
            player.GetCritChance(DamageClass.Generic) += 20f;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<Dusts.ShadowDust>());
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.4f, 0.8f);
            }
        }
    }

    public class LingYinPlayer : ModPlayer
    {
        public bool HasLingYin { get; set; }

        public override void ResetEffects()
        {
            HasLingYin = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasLingYin)
            {
                int buffType = ModContent.BuffType<LingYinBuff>();
                int buffIndex = Player.FindBuffIndex(buffType);
                if (buffIndex >= 0)
                {
                    Player.DelBuff(buffIndex);
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasLingYin)
            {
                int buffType = ModContent.BuffType<LingYinBuff>();
                int buffIndex = Player.FindBuffIndex(buffType);
                if (buffIndex >= 0)
                {
                    Player.DelBuff(buffIndex);
                }
            }
        }
    }
}
