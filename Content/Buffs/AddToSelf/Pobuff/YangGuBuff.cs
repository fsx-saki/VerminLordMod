using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YangGuBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.buffImmune[BuffID.Chilled] = true;
            player.buffImmune[BuffID.Frozen] = true;

            bool isDay = Main.dayTime;

            if (isDay)
            {
                player.GetDamage(DamageClass.Generic) += 0.10f;
                player.lifeRegen += 1;
            }
            else
            {
                player.lifeRegen += 1;
            }

            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.dayTime ? 1.2f : 0.8f;
            }
        }
    }
}
