using Terraria;
using Terraria.ID;
using VerminLordMod.Common.QuestSystem.Core;

namespace VerminLordMod.Common.QuestSystem.QLNodes
{
    public class YuanHaiQuest : QuestNode
    {
        public override void SetStaticDefaults()
        {
            Position = new Microsoft.Xna.Framework.Vector2(0, 160);
            QuestType = QuestType.Main;
            Difficulty = QuestDifficulty.Normal;
            AddParent<FirstQuest>();

            AddCollectObjective(50, ItemID.SilverCoin);
            AddReward(ItemID.ManaCrystal, 3);
            AddReward(ItemID.LifeCrystal, 1);
        }

        public override void UpdateByPlayer()
        {
            if (!IsUnlocked || IsCompleted) return;

            int count = 0;
            foreach (var item in Main.LocalPlayer.inventory)
            {
                if (!item.IsAir && item.type == ItemID.SilverCoin)
                    count += item.stack;
            }

            if (Objectives.Count > 0)
                Objectives[0].CurrentProgress = count;

            if (Objectives.TrueForAll(o => o.IsCompleted))
                IsCompleted = true;
        }
    }
}
