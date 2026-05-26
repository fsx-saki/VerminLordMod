using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class XuQingBuff : ModBuff
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
            var xuQingPlayer = player.GetModPlayer<global::VerminLordMod.Common.Players.XuQingPlayer>();
            if (xuQingPlayer.IsInvisible)
            {
                player.GetCritChance(DamageClass.Generic) += 15f;
                player.stealth = 0f;
            }

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Shadowflame);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.4f, 0.8f);
                d.alpha = 100;
            }
        }
    }
}
