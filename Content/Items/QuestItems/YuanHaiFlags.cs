using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Content.Items.QuestItems
{
    public class YuanHaiFlags : ModPlayer
    {
        public bool Activated { get; set; }
        public bool IsJiaGrade { get; set; }
        public bool BoxClaimed { get; set; }
        public int QiCurrent { get; set; } = 3;
        public int QiMax { get; set; } = 3;

        public override void SaveData(TagCompound tag)
        {
            tag["YH_Activated"] = Activated;
            tag["YH_IsJia"] = IsJiaGrade;
            tag["YH_BoxClaimed"] = BoxClaimed;
            tag["YH_QiCur"] = QiCurrent;
            tag["YH_QiMax"] = QiMax;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("YH_Activated", out bool a)) Activated = a;
            if (tag.TryGet("YH_IsJia", out bool j)) IsJiaGrade = j;
            if (tag.TryGet("YH_BoxClaimed", out bool c)) BoxClaimed = c;
            if (tag.TryGet("YH_QiCur", out int qc)) QiCurrent = qc;
            if (tag.TryGet("YH_QiMax", out int qm)) QiMax = qm;
        }
    }
}
