using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class FenYingBuff : ModBuff
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
            player.GetCritChance(DamageClass.Generic) += 10f;
            player.GetModPlayer<FenYingPlayer>().HasFenYing = true;
        }
    }

    public class FenYingPlayer : ModPlayer
    {
        public bool HasFenYing { get; set; }

        public override void ResetEffects()
        {
            HasFenYing = false;
        }

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (HasFenYing && Main.rand.NextFloat() < 0.15f)
            {
                modifiers.FinalDamage *= 0f;
                Player.SetImmuneTimeForAllTypes(Player.longInvince ? 60 : 30);
            }
        }
    }
}
