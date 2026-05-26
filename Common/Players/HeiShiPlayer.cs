using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Players
{
    public class HeiShiPlayer : ModPlayer
    {
        public int BonusDefenseCount { get; set; }

        public override void ResetEffects()
        {
        }

        public override void PostUpdateEquips()
        {
            if (BonusDefenseCount > 0)
            {
                Player.statDefense += BonusDefenseCount;
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag["bonusDefenseCount"] = BonusDefenseCount;
        }

        public override void LoadData(TagCompound tag)
        {
            BonusDefenseCount = tag.GetInt("bonusDefenseCount");
        }
    }
}
