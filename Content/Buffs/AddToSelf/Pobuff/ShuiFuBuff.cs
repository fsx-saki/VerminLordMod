using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ShuiFuBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = true;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetDamage(DamageClass.Generic) += 0.1f;
            player.GetDamage(ModContent.GetInstance<InsectDamageClass>()) += 0.1f;
            player.moveSpeed += 0.1f;
            player.GetAttackSpeed(DamageClass.Generic) += 0.1f;
            player.GetCritChance(DamageClass.Generic) += 10;
            player.lifeRegen += 2;

            if (Main.rand.NextBool(20))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    DustID.PurpleTorch, 0, -0.5f, Scale: 0.8f);
                d.noGravity = true;
            }
        }
    }
}
