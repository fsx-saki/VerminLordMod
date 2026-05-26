using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class XingNianXianBuff : ModBuff
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
            bool isNight = !Main.dayTime;

            if (isNight)
            {
                player.GetDamage(DamageClass.Generic) += 0.20f;
                player.GetCritChance(DamageClass.Generic) += 15f;
                var qiResource = player.GetModPlayer<QiResourcePlayer>();
                qiResource.ExtraQiRegen += qiResource.BaseQiRegenRate * 0.10f;
            }
            else
            {
                player.GetDamage(DamageClass.Generic) += 0.08f;
                player.GetCritChance(DamageClass.Generic) += 5f;
            }

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }
}
