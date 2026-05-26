using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class SiGuBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.20f;
            player.GetModPlayer<SiGuPlayer>().HasSiGu = true;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<DarkDust>());
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class SiGuPlayer : ModPlayer
    {
        public bool HasSiGu { get; set; }

        public override void ResetEffects()
        {
            HasSiGu = false;
        }

        public override void PostUpdate()
        {
            if (HasSiGu)
            {
                Player.lifeRegen -= 3;
            }
        }
    }
}
