using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Players
{
    public class HeiBaiShiPlayer : ModPlayer
    {
        public int BonusMeleeDamageCount { get; set; }

        public override void ResetEffects()
        {
        }

        public override void PostUpdateEquips()
        {
            if (BonusMeleeDamageCount > 0)
            {
                Player.GetDamage(DamageClass.Melee) += 0.02f * BonusMeleeDamageCount;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["bonusMeleeDamageCount"] = BonusMeleeDamageCount;
        }

        public override void LoadData(TagCompound tag)
        {
            BonusMeleeDamageCount = tag.GetInt("bonusMeleeDamageCount");
        }
    }
}
