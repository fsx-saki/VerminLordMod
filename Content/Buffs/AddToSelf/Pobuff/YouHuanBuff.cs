using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YouHuanBuff : ModBuff
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

            if (Main.rand.NextBool(60))
            {
                player.Hurt(Terraria.DataStructures.PlayerDeathReason.LegacyDefault(), 1, 0);
            }

            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Shadowflame);
                d.velocity *= 0.5f;
                d.noGravity = true;
            }
        }
    }
}
