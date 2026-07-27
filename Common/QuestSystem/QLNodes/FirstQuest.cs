using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.QuestSystem.Core;
using VerminLordMod.Content.Items.QuestItems;

namespace VerminLordMod.Common.QuestSystem.QLNodes
{
    public class FirstQuest : QuestNode
    {
        public override void SetStaticDefaults()
        {
            Position = new Microsoft.Xna.Framework.Vector2(0, 0);
            QuestType = QuestType.Main;
            Difficulty = QuestDifficulty.Easy;

            Objectives.Add(new QuestObjective
            {
                Description = this.GetLocalization("QuestObjective.Description", () => "点击领取"),
                RequiredProgress = 1
            });

            AddReward(ModContent.ItemType<MoonlightGu>(), 1);
            AddReward(ModContent.ItemType<YuanShi>(), 20);
            AddReward(ModContent.ItemType<ElementTester>(), 1);
            AddReward(ItemID.Rope, 100);
            AddReward(ItemID.Torch, 50);
            AddReward(ItemID.RecallPotion, 5);
        }

        public override void OnWorldEnter()
        {
            if (IsUnlocked && Objectives.Count > 0)
                Objectives[0].CurrentProgress = 1;
        }

        public override void UpdateByPlayer()
        {
            if (!IsUnlocked || IsCompleted) return;
            Objectives[0].CurrentProgress = 1;
            if (Objectives.TrueForAll(o => o.IsCompleted))
                IsCompleted = true;
        }
    }
}
