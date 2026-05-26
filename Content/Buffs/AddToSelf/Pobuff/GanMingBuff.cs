using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class GanMingBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.30f;
            player.GetCritChance(DamageClass.Generic) += 20;
            player.GetAttackSpeed(DamageClass.Generic) += 0.15f;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Blood);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch);
                d.velocity.Y -= 1.5f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }
}
