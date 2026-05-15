using System.Collections.Generic;
using Microsoft.Xna.Framework;
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

        public const int BASE_GROWTH_TIME_GU_BOX = 36000;
        public const int BASE_GROWTH_TIME_GU_CAULDRON = 72000;
        public const int BASE_GROWTH_TIME_SPIRIT_POOL = 54000;
        public const int BASE_GROWTH_TIME_BATTLE_ARENA = 18000;
        public const int BASE_GROWTH_TIME_ANCESTRAL_ALTAR = 108000;

        public override void OnWorldLoad()
        {
        }

        public override void PostUpdateWorld()
        {
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                var player = Main.player[i];
                if (!player.active) continue;

                var breedingPlayer = player.GetModPlayer<GuBreedingPlayer>();
                for (int j = 0; j < breedingPlayer.BreedingSlots.Count; j++)
                {
                    var slot = breedingPlayer.BreedingSlots[j];
                    if (slot.Stage != BreedingStage.Growing && slot.Stage != BreedingStage.Competing)
                        continue;

                    float growthRate = CalculateGrowthRate(slot);
                    slot.GrowthTicks += (int)(growthRate * 60f);

                    if (slot.Stage == BreedingStage.Growing && slot.GrowthTicks >= slot.RequiredGrowthTicks)
                    {
                        slot.Stage = BreedingStage.Ready;
                        if (player.whoAmI == Main.myPlayer)
                            Main.NewText("蛊虫培养完成！可以收获了。", Color.Green);
                    }

                    if (slot.Stage == BreedingStage.Competing && slot.GrowthTicks >= slot.RequiredGrowthTicks)
                    {
                        ResolveCompetition(slot);
                        slot.Stage = BreedingStage.Ready;
                        if (player.whoAmI == Main.myPlayer)
                            Main.NewText("蛊虫竞争结束！胜者已诞生。", Color.Orange);
                    }

                    breedingPlayer.BreedingSlots[j] = slot;
                }
            }
        }

        private float CalculateGrowthRate(BreedingSlot slot)
        {
            float rate = 1f;
            rate *= 0.5f + slot.NutritionLevel;
            float tempFactor = 1f - System.Math.Abs(slot.Temperature - 25f) / 50f;
            rate *= System.Math.Max(0.3f, tempFactor);
            return rate;
        }

        public BreedingSlot CreateSlot(BreedingContainerType container, int requiredGuLevel)
        {
            return new BreedingSlot
            {
                Container = container,
                Stage = BreedingStage.Empty,
                RequiredGrowthTicks = GetGrowthTime(container),
                RequiredGuLevel = requiredGuLevel,
                Temperature = 25f,
            };
        }

        private int GetGrowthTime(BreedingContainerType container)
        {
            return container switch
            {
                BreedingContainerType.GuBox => BASE_GROWTH_TIME_GU_BOX,
                BreedingContainerType.GuCauldron => BASE_GROWTH_TIME_GU_CAULDRON,
                BreedingContainerType.SpiritPoolBox => BASE_GROWTH_TIME_SPIRIT_POOL,
                BreedingContainerType.BattleArena => BASE_GROWTH_TIME_BATTLE_ARENA,
                BreedingContainerType.AncestralAltar => BASE_GROWTH_TIME_ANCESTRAL_ALTAR,
                _ => BASE_GROWTH_TIME_GU_BOX
            };
        }

        public bool CanSeedSlot(Player player, BreedingSlot slot, int guItemType)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel < slot.RequiredGuLevel) return false;
            if (slot.Stage != BreedingStage.Empty) return false;
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
            slot.NutritionLevel = System.Math.Min(2f, slot.NutritionLevel + 0.15f);
        }

        public int HarvestSlot(BreedingSlot slot)
        {
            if (slot.Stage != BreedingStage.Ready) return -1;

            int resultItemType = CalculateOutput(slot);
            slot.GuItemTypes.Clear();
            slot.Stage = BreedingStage.Empty;
            slot.NutritionLevel = 0f;
            slot.GrowthTicks = 0;
            return resultItemType;
        }

        private int CalculateOutput(BreedingSlot slot)
        {
            if (slot.GuItemTypes.Count == 0) return 0;

            int winnerIndex = 0;
            if (slot.GuItemTypes.Count > 1)
            {
                winnerIndex = Main.rand.Next(slot.GuItemTypes.Count);
            }

            return slot.GuItemTypes[winnerIndex];
        }

        public void TriggerCompetition(BreedingSlot slot)
        {
            if (slot.GuItemTypes.Count < 2)
            {
                Main.NewText("需要至少两只蛊虫才能进行竞争！", Color.Gray);
                return;
            }

            slot.Stage = BreedingStage.Competing;
            slot.RequiredGrowthTicks = GetGrowthTime(BreedingContainerType.BattleArena);
            slot.GrowthTicks = 0;
        }

        private void ResolveCompetition(BreedingSlot slot)
        {
            if (slot.GuItemTypes.Count <= 1) return;

            int strongestIndex = 0;
            float strongestPower = EvaluateGuPower(slot.GuItemTypes[0]);

            for (int i = 1; i < slot.GuItemTypes.Count; i++)
            {
                float power = EvaluateGuPower(slot.GuItemTypes[i]);
                if (power > strongestPower)
                {
                    strongestPower = power;
                    strongestIndex = i;
                }
            }

            int winnerType = slot.GuItemTypes[strongestIndex];
            slot.GuItemTypes.Clear();
            slot.GuItemTypes.Add(winnerType);

            if (Main.rand.NextFloat() < 0.15f * slot.NutritionLevel)
            {
                int evolvedType = TryGetEvolvedForm(winnerType);
                if (evolvedType > 0)
                {
                    slot.GuItemTypes.Clear();
                    slot.GuItemTypes.Add(evolvedType);
                }
            }
        }

        private float EvaluateGuPower(int guItemType)
        {
            float basePower = 100f;
            var item = new Item(guItemType);
            if (item.damage > 0) basePower += item.damage * 2f;
            if (item.rare > 0) basePower += item.rare * 50f;
            basePower += Main.rand.NextFloat(-20f, 20f);
            return basePower;
        }

        private int TryGetEvolvedForm(int guItemType)
        {
            if (GuEvolutionSystem.Instance != null &&
                GuEvolutionSystem.Instance.EvolutionPaths.TryGetValue(guItemType, out var paths) &&
                paths.Count > 0)
            {
                return paths[0].TargetGuType;
            }
            return 0;
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