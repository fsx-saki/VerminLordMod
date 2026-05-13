using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // PlantingSystem — 种植/药园系统大框
    //
    // 系统定位：
    // 种植系统让玩家可以在领地内种植药材、蛊虫食物、灵草。
    // 对应小说中的药园/蛊虫饲养场。
    //
    // 功能规划：
    // 1. 可种植作物类型：药材（月兰/月兰草等）、蛊虫食物（蛊草）、灵草
    // 2. 种植条件：需要对应的种植台/药园Tile + 土壤 + 水源
    // 3. 生长阶段：种子→幼苗→成长→成熟→可收获
    // 4. 生长时间：不同作物有不同的生长周期
    // 5. 生长影响因素：环境（生态区）、温度、灵池加成
    // 6. 收获产出：成熟后收获物品
    // 7. 药园管理：NPC（药堂弟子）可自动管理药园
    //
    // TODO:
    //   - 创建种植Tile（药园Tile、蛊草Tile）
    //   - 创建种子物品
    //   - 实现生长阶段逻辑
    //   - 实现收获逻辑
    //   - 实现生长影响因素
    //   - 实现药园管理NPC交互
    // ============================================================

    public enum PlantType
    {
        MoonOrchid,         // 月兰 - 药材，炼丹基础材料
        RiceBagGrass,       // 稻草蛊草 - 蛊虫食物
        WineGourdFlower,    // 酒壶花 - 蛊虫食物
        SpiritGrass,        // 灵草 - 修炼辅助
        SpearBamboo,        // 枪竹 - 武器材料
        HealingHerb,        // 疗伤草 - 药材
        QiHerb,             // 元草 - 真元回复药材
        PoisonWeed,         // 毒草 - 蛊毒类药材
        BoneBanboo,         // 骨竹 - 建筑材料+蛊虫栖息
        SpiritSpringPlant   // 灵泉花 - 灵池加速
    }

    public enum PlantStage
    {
        Seed,               // 种子期
        Seedling,           // 幼苗期
        Growing,            // 成长期
        Mature,             // 成熟期
        Harvestable         // 可收获
    }

    public class PlantInstance
    {
        public PlantType Type;
        public PlantStage Stage;
        public Microsoft.Xna.Framework.Vector2 Position;    // Tile坐标
        public int GrowthTicks;            // 已生长的帧数
        public int RequiredGrowthTicks;    // 需要的总生长帧数
        public float GrowthRateMultiplier; // 生长速率倍率（受环境影响）
        public int HarvestItem;            // 收获产出物品Type
        public int HarvestAmount;          // 收获数量
        public bool IsWatered;             // 是否已浇水
    }

    public class PlantingSystem : ModSystem
    {
        public static PlantingSystem Instance => ModContent.GetInstance<PlantingSystem>();

        public List<PlantInstance> ActivePlants = new();

        public override void OnWorldLoad()
        {
            ActivePlants.Clear();
        }

        public override void PreUpdateWorld()
        {
            // 更新植物生长
            foreach (var plant in ActivePlants)
            {
                if (plant.Stage == PlantStage.Harvestable) continue;

                float rate = plant.GrowthRateMultiplier;
                if (plant.IsWatered) rate *= 1.5f;

                plant.GrowthTicks += (int)(rate);
                UpdatePlantStage(plant);
            }
        }

        private void UpdatePlantStage(PlantInstance plant)
        {
            float progress = plant.GrowthTicks / (float)plant.RequiredGrowthTicks;
            plant.Stage = progress switch
            {
                >= 1.0f => PlantStage.Harvestable,
                >= 0.75f => PlantStage.Mature,
                >= 0.5f => PlantStage.Growing,
                >= 0.25f => PlantStage.Seedling,
                _ => PlantStage.Seed
            };
        }

        public PlantInstance PlantSeed(PlantType type, Microsoft.Xna.Framework.Vector2 position)
        {
            var plant = new PlantInstance
            {
                Type = type,
                Stage = PlantStage.Seed,
                Position = position,
                GrowthTicks = 0,
                RequiredGrowthTicks = GetGrowthTime(type),
                GrowthRateMultiplier = 1f,
                HarvestItem = GetHarvestItem(type),
                HarvestAmount = GetHarvestAmount(type)
            };

            ActivePlants.Add(plant);
            return plant;
        }

        private int GetGrowthTime(PlantType type)
        {
            return type switch
            {
                PlantType.MoonOrchid => 72000,       // 2天
                PlantType.HealingHerb => 54000,       // 1.5天
                PlantType.QiHerb => 90000,            // 2.5天
                PlantType.RiceBagGrass => 36000,      // 1天
                PlantType.SpiritGrass => 180000,      // 5天
                _ => 72000
            };
        }

        private int GetHarvestItem(PlantType type)
        {
            // TODO: 返回对应的收获物品Type
            return 0;
        }

        private int GetHarvestAmount(PlantType type)
        {
            return type switch
            {
                PlantType.MoonOrchid => 3,
                PlantType.HealingHerb => 5,
                PlantType.QiHerb => 2,
                _ => 3
            };
        }

        public void Harvest(PlantInstance plant, Player player)
        {
            if (plant.Stage != PlantStage.Harvestable) return;
            // TODO: 产出物品给玩家
            ActivePlants.Remove(plant);
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: 保存植物数据
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载植物数据
        }
    }
}