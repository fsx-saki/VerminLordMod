using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // GuBreedingSystem — 蛊虫养殖/孵化系统大框
    //
    // 系统定位：
    // 蛊虫是蛊师的核心资源，养殖系统让玩家可以培育蛊虫。
    // 对应小说中的养蛊过程：喂食、环境、温度、竞争。
    //
    // 功能规划：
    // 1. 蛊虫孵化槽（类似空窍但用于培养而非战斗）
    // 2. 培养条件：食物（对应蛊虫属性）、环境温度、养蛊时间
    // 3. 蛊虫竞争：多个蛊虫同槽可产生竞争/吞噬 → 进化
    // 4. 蛊虫品质：野生 vs 养殖，养殖可控品质
    // 5. 培养容器：蛊盒（低级）/ 蛊蛊鼎（高级）
    // 6. 培养结果：产出新的蛊虫物品
    //
    // TODO:
    //   - 实现蛊虫培养数据结构
    //   - 实现培养容器Tile（蛊盒、蛊鼎）
    //   - 实现培养UI
    //   - 实现蛊虫竞争/吞噬逻辑
    //   - 实现培养时间进度
    //   - 创建培养容器物品
    // ============================================================

    public enum BreedingContainerType
    {
        GuBox,              // 蛊盒 - 一转蛊师可用，培养一转蛊虫
        GuCauldron,         // 蛊蛊鼎 - 三转蛊师可用，培养二转以上
        SpiritPoolBox,      // 灵池蛊盒 - 药脉专用，培养治疗类蛊虫
        BattleArena,        // 斗蛊台 - 专用于蛊虫竞争/吞噬
        AncestralAltar      // 祭坛 - 族长级，培养稀有蛊虫
    }

    public enum BreedingStage
    {
        Empty,              // 空槽
        Seeded,             // 已投入蛊虫种子/幼蛊
        Growing,            // 培养中
        Ready,              // 可收获
        Competing           // 蛊虫竞争阶段
    }

    public class BreedingSlot
    {
        public BreedingContainerType Container;
        public BreedingStage Stage;
        public List<int> GuItemTypes = new();    // 槽中的蛊虫
        public int FoodItemType;                 // 当前喂食的蛊虫食物
        public int GrowthTicks;                  // 培养进度
        public int RequiredGrowthTicks;          // 需要的总培养时间
        public float Temperature;                // 环境温度（影响培养效率）
        public float NutritionLevel;             // 营养水平
        public int RequiredGuLevel;              // 使用此槽的最低修为
    }

    public class GuBreedingSystem : ModSystem
    {
        public static GuBreedingSystem Instance => ModContent.GetInstance<GuBreedingSystem>();

        public override void OnWorldLoad()
        {
            // TODO: 初始化养殖系统
        }

        public BreedingSlot CreateSlot(BreedingContainerType container, int requiredGuLevel)
        {
            return new BreedingSlot
            {
                Container = container,
                Stage = BreedingStage.Empty,
                RequiredGrowthTicks = GetGrowthTime(container),
                RequiredGuLevel = requiredGuLevel
            };
        }

        private int GetGrowthTime(BreedingContainerType container)
        {
            // TODO: 完善培养时间计算
            return container switch
            {
                BreedingContainerType.GuBox => 36000,
                BreedingContainerType.GuCauldron => 72000,
                BreedingContainerType.SpiritPoolBox => 54000,
                _ => 36000
            };
        }

        public bool CanSeedSlot(Player player, BreedingSlot slot, int guItemType)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel < slot.RequiredGuLevel) return false;
            if (slot.Stage != BreedingStage.Empty) return false;
            // TODO: 检查蛊虫与容器兼容性
            return true;
        }

        public void SeedSlot(BreedingSlot slot, int guItemType)
        {
            slot.GuItemTypes.Add(guItemType);
            slot.Stage = BreedingStage.Growing;
            slot.GrowthTicks = 0;
        }

        public void FeedSlot(BreedingSlot slot, int foodItemType)
        {
            slot.FoodItemType = foodItemType;
            slot.NutritionLevel += 0.1f;
        }

        public int HarvestSlot(BreedingSlot slot)
        {
            if (slot.Stage != BreedingStage.Ready) return -1;
            // TODO: 根据培养条件计算产出蛊虫类型和品质
            int resultItemType = CalculateOutput(slot);
            slot.GuItemTypes.Clear();
            slot.Stage = BreedingStage.Empty;
            return resultItemType;
        }

        private int CalculateOutput(BreedingSlot slot)
        {
            // TODO: 完善产出计算逻辑
            // 营养水平 → 品质加成
            // 温度 → 特定属性倾向
            // 蛊虫竞争 → 可能吞噬产出更高阶蛊虫
            return 0;
        }

        public void TriggerCompetition(BreedingSlot slot)
        {
            slot.Stage = BreedingStage.Competing;
            // TODO: 蛊虫竞争/吞噬逻辑
            // 最强者存活，弱者被吞噬
            // 吞噬后可能进化
        }
    }

    // ============================================================
    // GuBreedingPlayer — 玩家养殖数据追踪
    // ============================================================

    public class GuBreedingPlayer : ModPlayer
    {
        public List<BreedingSlot> BreedingSlots = new();
        public int MaxBreedingSlots = 2;         // 初始2个槽位

        public void ExpandBreedingSlots(int additional)
        {
            MaxBreedingSlots += additional;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["MaxBreedingSlots"] = MaxBreedingSlots;
        }

        public override void LoadData(TagCompound tag)
        {
            MaxBreedingSlots = tag.GetInt("MaxBreedingSlots");
        }
    }
}