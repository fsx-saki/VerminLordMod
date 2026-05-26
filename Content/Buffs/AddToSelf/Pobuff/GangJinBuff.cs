using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class GangJinBuff : ModBuff
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
            player.statDefense += 5;
            player.GetModPlayer<GangJinPlayer>().HasGangJin = true;
        }
    }

    public class GangJinPlayer : ModPlayer
    {
        public bool HasGangJin { get; set; }

        public override void ResetEffects()
        {
            HasGangJin = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasGangJin)
            {
                modifiers.FinalDamage *= 0.97f;
            }
        }
    }
}
