using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.QuestSystem.Core
{
    public class QuestSaveData
    {
        public bool IsUnlocked;
        public bool IsCompleted;
        public List<int> ObjectiveProgress = [];
        public List<bool> RewardsClaimed = [];

        public static QuestSaveData Default => new()
        {
            IsUnlocked = false,
            IsCompleted = false,
            ObjectiveProgress = [],
            RewardsClaimed = []
        };

        public TagCompound Serialize()
        {
            return new TagCompound
            {
                ["IsUnlocked"] = IsUnlocked,
                ["IsCompleted"] = IsCompleted,
                ["ObjectiveProgress"] = ObjectiveProgress,
                ["RewardsClaimed"] = RewardsClaimed
            };
        }

        public static QuestSaveData Deserialize(TagCompound tag)
        {
            var data = new QuestSaveData();
            if (tag.TryGet("IsUnlocked", out bool isUnlocked))
                data.IsUnlocked = isUnlocked;
            if (tag.TryGet("IsCompleted", out bool isCompleted))
                data.IsCompleted = isCompleted;
            if (tag.TryGet("ObjectiveProgress", out List<int> objectiveProgress))
                data.ObjectiveProgress = objectiveProgress;
            if (tag.TryGet("RewardsClaimed", out List<bool> rewardsClaimed))
                data.RewardsClaimed = rewardsClaimed;
            return data;
        }
    }
}
