using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class XingNianBuff : ModBuff
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
            float damageMult = isNight ? 0.30f : 0.075f;
            float critMult = isNight ? 0.10f : 0.025f;

            player.GetDamage(DamageClass.Magic) += damageMult;
            player.GetCritChance(DamageClass.Magic) += critMult;

            if (isNight && Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = 0.7f;
            }
        }
    }
}
