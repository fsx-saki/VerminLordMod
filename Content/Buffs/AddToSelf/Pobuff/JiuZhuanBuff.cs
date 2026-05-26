using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JiuZhuanBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
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
            player.GetCritChance(DamageClass.Generic) += 10f;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ExtraQiRegen += qiResource.BaseQiRegenRate * 0.08f;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Enchanted_Gold);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }
        }
    }
}
