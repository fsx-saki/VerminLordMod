using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Players
{
    public class QingTongSheLiPlayer : ModPlayer
    {
        public int PermanentDefenseBonus { get; set; }

        public override void Initialize()
        {
            PermanentDefenseBonus = 0;
        }

        public override void PostUpdate()
        {
            Player.statDefense += PermanentDefenseBonus;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["PermanentDefenseBonus"] = PermanentDefenseBonus;
        }

        public override void LoadData(TagCompound tag)
        {
            PermanentDefenseBonus = tag.GetInt("PermanentDefenseBonus");
        }
    }
}
