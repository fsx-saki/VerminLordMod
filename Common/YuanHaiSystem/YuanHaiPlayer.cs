using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.YuanHaiSystem
{
    public class YuanHaiPlayer : ModPlayer
    {
        public List<GuData> GuCollection = [];
        public int QiCurrent = 80;
        public int QiMax = 80;

        public bool HasRefiningSlot;
        public int RefiningItemType;
        public int RefiningYuanShiProgress;
        public int RefiningYuanShiRequired = 10;
        public int RefiningTimer;
        public bool RefiningComplete;

        public void AddGu(int itemType)
        {
            var personality = (GuPersonality)(itemType % 5);
            GuCollection.Add(new GuData(itemType, personality));
        }

        public void RemoveGu(int index)
        {
            if (index >= 0 && index < GuCollection.Count)
                GuCollection.RemoveAt(index);
        }

        public override void SaveData(TagCompound tag)
        {
            var list = new List<TagCompound>();
            foreach (var gu in GuCollection)
                list.Add(gu.Serialize());
            tag["YH_GuList"] = list;
            tag["YH_QiCur"] = QiCurrent;
            tag["YH_QiMax"] = QiMax;
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("YH_GuList", out List<TagCompound> list))
            {
                GuCollection = [];
                foreach (var t in list)
                    GuCollection.Add(GuData.Deserialize(t));
            }
            if (tag.TryGet("YH_QiCur", out int qc)) QiCurrent = qc;
            if (tag.TryGet("YH_QiMax", out int qm)) QiMax = qm;
        }
    }
}
