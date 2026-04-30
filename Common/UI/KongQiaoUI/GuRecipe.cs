using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Items.Consumables;

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 蛊虫配方 — 描述合炼所需的材料、消耗和产物
    /// </summary>
    public class GuRecipe
    {
        /// <summary>产物物品类型</summary>
        public int ResultType { get; set; }

        /// <summary>产物名称</summary>
        public string ResultName { get; set; }

        /// <summary>产物蛊虫等级（一转=1，二转=2...）</summary>
        public int GuLevel { get; set; }

        /// <summary>合炼消耗真元</summary>
        public int QiCost { get; set; }

        /// <summary>所需材料 {物品类型, 数量}</summary>
        public Dictionary<int, int> Materials { get; set; } = new();

        /// <summary>配方类别（用于UI分类）</summary>
        public RecipeCategory Category { get; set; } = RecipeCategory.General;

        /// <summary>配方描述</summary>
        public string Description { get; set; } = "";

        /// <summary>是否启用（可动态禁用）</summary>
        public bool Enabled { get; set; } = true;

        public Dictionary<int, int> GetMaterials() => Materials;
    }

    /// <summary>
    /// 配方类别
    /// </summary>
    public enum RecipeCategory
    {
        /// <summary>通用/其他</summary>
        General,

        /// <summary>力道蛊虫（斤力蛊、钧力蛊等）</summary>
        Strength,

        /// <summary>豕蛊（白豕蛊、黑豕蛊等）</summary>
        PigGu,

        /// <summary>酒虫系列</summary>
        WineBug,

        /// <summary>破境丹（一转→二转等）</summary>
        RealmBreak,

        /// <summary>沙砾系列</summary>
        Shari,

        /// <summary>天牛系列</summary>
        Longicorn,

        /// <summary>资质系列（子资质甲乙丙丁）</summary>
        ZiZhi,

        /// <summary>其他消耗品</summary>
        Consumable,
    }

    /// <summary>
    /// 蛊虫合成系统 — 管理所有配方注册和查询
    /// </summary>
    public static class GuCraftSystem
    {
        private static List<GuRecipe> _recipes;
        private static bool _initialized;

        /// <summary>
        /// 初始化所有配方（线程安全，可重复调用）
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _recipes = new List<GuRecipe>
            {
                // ========== 一转蛊虫 ==========

                // 白豕蛊 — 一转珍稀豕蛊，永久加力1%
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<WhitePigGu>(),
                    ResultName = "白豕蛊",
                    GuLevel = 1,
                    QiCost = 30,
                    Category = RecipeCategory.PigGu,
                    Description = "一转珍稀豕蛊，永久增加近战伤害1%（上限一猪之力）",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.PiggyBank, 1 },
                        { ItemID.Bone, 15 },
                        { ItemID.Gel, 30 },
                    }
                },

                // 黑豕蛊 — 一转珍稀豕蛊，永久加力1%
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<BlackPigGu>(),
                    ResultName = "黑豕蛊",
                    GuLevel = 1,
                    QiCost = 30,
                    Category = RecipeCategory.PigGu,
                    Description = "一转珍稀豕蛊，永久增加近战伤害1%（上限一猪之力，可与白豕蛊叠加）",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.PiggyBank, 1 },
                        { ItemID.ShadowScale, 15 },
                        { ItemID.Gel, 30 },
                    }
                },

                // 斤力蛊 — 一转力道蛊
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<JinLiGu>(),
                    ResultName = "斤力蛊",
                    GuLevel = 1,
                    QiCost = 20,
                    Category = RecipeCategory.Strength,
                    Description = "一转力道蛊，永久增加近战伤害1%",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.IronBar, 5 },
                        { ItemID.Wood, 20 },
                    }
                },

                // 酒虫 — 一转珍稀蛊虫
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<WineBug>(),
                    ResultName = "酒虫",
                    GuLevel = 1,
                    QiCost = 25,
                    Category = RecipeCategory.WineBug,
                    Description = "一转珍稀蛊虫，精炼真元，永久增加真元恢复速度+1",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.Ale, 5 },
                        { ItemID.BottledHoney, 1 },
                        { ItemID.Mushroom, 10 },
                    }
                },

                // ========== 二转蛊虫 ==========

                // 钧力蛊 — 二转力道蛊
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<JunLiGu>(),
                    ResultName = "钧力蛊",
                    GuLevel = 2,
                    QiCost = 80,
                    Category = RecipeCategory.Strength,
                    Description = "二转力道蛊，永久增加近战伤害10%",
                    Materials = new Dictionary<int, int>
                    {
                        { ModContent.ItemType<JinLiGu>(), 3 },
                        { ItemID.GoldBar, 5 },
                        { ItemID.SoulofMight, 2 },
                    }
                },

                // 十斤力蛊 — 二转力道蛊
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<ShiJinLiGu>(),
                    ResultName = "十斤力蛊",
                    GuLevel = 2,
                    QiCost = 100,
                    Category = RecipeCategory.Strength,
                    Description = "二转力道蛊，永久增加近战伤害10%（相当于10只斤力蛊）",
                    Materials = new Dictionary<int, int>
                    {
                        { ModContent.ItemType<JinLiGu>(), 10 },
                        { ItemID.SoulofNight, 3 },
                    }
                },

                // 四味酒虫 — 二转珍稀蛊虫
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<FourFlavorWineBug>(),
                    ResultName = "四味酒虫",
                    GuLevel = 2,
                    QiCost = 60,
                    Category = RecipeCategory.WineBug,
                    Description = "二转珍稀蛊虫，精炼真元，永久增加真元恢复速度+2（需先使用酒虫）",
                    Materials = new Dictionary<int, int>
                    {
                        { ModContent.ItemType<WineBug>(), 1 },
                        { ItemID.Lemonade, 1 },
                        { ItemID.BottledHoney, 1 },
                        { ItemID.LovePotion, 1 },
                        { ItemID.Ale, 1 },
                    }
                },

                // ========== 三转蛊虫 ==========

                // 十钧力蛊 — 三转力道蛊
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<ShiJunLiGu>(),
                    ResultName = "十钧力蛊",
                    GuLevel = 3,
                    QiCost = 400,
                    Category = RecipeCategory.Strength,
                    Description = "三转力道蛊，永久增加近战伤害100%（相当于10只钧力蛊）",
                    Materials = new Dictionary<int, int>
                    {
                        { ModContent.ItemType<JunLiGu>(), 10 },
                        { ItemID.SoulofFright, 5 },
                        { ItemID.ChlorophyteBar, 10 },
                    }
                },

                // 七香酒虫 — 三转珍稀蛊虫
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<SevenWineBug>(),
                    ResultName = "七香酒虫",
                    GuLevel = 3,
                    QiCost = 150,
                    Category = RecipeCategory.WineBug,
                    Description = "三转珍稀蛊虫，精炼真元，永久增加真元恢复速度+4（需先使用四味酒虫）",
                    Materials = new Dictionary<int, int>
                    {
                        { ModContent.ItemType<FourFlavorWineBug>(), 2 },
                        { ItemID.LifeforcePotion, 1 },
                        { ItemID.InfernoPotion, 1 },
                        { ItemID.WarmthPotion, 1 },
                        { ItemID.TitanPotion, 1 },
                        { ItemID.InvisibilityPotion, 1 },
                        { ItemID.WaterWalkingPotion, 1 },
                        { ItemID.MagicPowerPotion, 1 },
                    }
                },

                // ========== 四转蛊虫 ==========

                // 九眼酒虫 — 四转珍稀蛊虫
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<NineWineBug>(),
                    ResultName = "九眼酒虫",
                    GuLevel = 4,
                    QiCost = 300,
                    Category = RecipeCategory.WineBug,
                    Description = "四转珍稀蛊虫，提纯黄金真元，永久增加真元恢复速度+8（需先使用七香酒虫）",
                    Materials = new Dictionary<int, int>
                    {
                        { ModContent.ItemType<SevenWineBug>(), 2 },
                        { ItemID.KingSlimeTrophy, 1 },
                        { ItemID.EyeofCthulhuTrophy, 1 },
                        { ItemID.EaterofWorldsTrophy, 1 },
                        { ItemID.SkeletronTrophy, 1 },
                        { ItemID.QueenBeeTrophy, 1 },
                        { ItemID.WallofFleshTrophy, 1 },
                        { ItemID.DestroyerTrophy, 1 },
                        { ItemID.SkeletronPrimeTrophy, 1 },
                        { ItemID.RetinazerTrophy, 1 },
                    }
                },

                // ========== 破境丹系列 ==========

                // 一转→二转破境丹
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<FirstToSecond>(),
                    ResultName = "一转破境丹",
                    GuLevel = 1,
                    QiCost = 50,
                    Category = RecipeCategory.RealmBreak,
                    Description = "一转巅峰（一转后期）时使用，突破至二转",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.FallenStar, 5 },
                        { ItemID.SoulofLight, 3 },
                        { ItemID.SoulofNight, 3 },
                        { ItemID.BottledWater, 1 },
                    }
                },

                // 二转→三转破境丹
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<SecondToThird>(),
                    ResultName = "二转破境丹",
                    GuLevel = 2,
                    QiCost = 150,
                    Category = RecipeCategory.RealmBreak,
                    Description = "二转巅峰（二转后期）时使用，突破至三转",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.FallenStar, 10 },
                        { ItemID.SoulofMight, 5 },
                        { ItemID.SoulofSight, 5 },
                        { ItemID.SoulofFright, 5 },
                        { ItemID.BottledWater, 1 },
                    }
                },

                // 三转→四转破境丹
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<ThirdToForth>(),
                    ResultName = "三转破境丹",
                    GuLevel = 3,
                    QiCost = 300,
                    Category = RecipeCategory.RealmBreak,
                    Description = "三转巅峰（三转后期）时使用，突破至四转",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.FallenStar, 20 },
                        { ItemID.HallowedBar, 10 },
                        { ItemID.SoulofMight, 10 },
                        { ItemID.SoulofSight, 10 },
                        { ItemID.SoulofFright, 10 },
                        { ItemID.BottledWater, 1 },
                    }
                },

                // 四转→五转破境丹
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<ForthToFifth>(),
                    ResultName = "四转破境丹",
                    GuLevel = 4,
                    QiCost = 500,
                    Category = RecipeCategory.RealmBreak,
                    Description = "四转巅峰（四转后期）时使用，突破至五转",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.FallenStar, 30 },
                        { ItemID.ChlorophyteBar, 15 },
                        { ItemID.Ectoplasm, 10 },
                        { ItemID.BottledWater, 1 },
                    }
                },

                // 五转→六转破境丹
                new GuRecipe
                {
                    ResultType = ModContent.ItemType<FifthToSixth>(),
                    ResultName = "五转破境丹",
                    GuLevel = 5,
                    QiCost = 800,
                    Category = RecipeCategory.RealmBreak,
                    Description = "五转巅峰（五转后期）时使用，突破至六转",
                    Materials = new Dictionary<int, int>
                    {
                        { ItemID.FallenStar, 50 },
                        { ItemID.LunarBar, 10 },
                        { ItemID.FragmentSolar, 10 },
                        { ItemID.FragmentVortex, 10 },
                        { ItemID.FragmentNebula, 10 },
                        { ItemID.FragmentStardust, 10 },
                        { ItemID.BottledWater, 1 },
                    }
                },
            };
        }

        /// <summary>
        /// 获取所有配方
        /// </summary>
        public static List<GuRecipe> GetAllRecipes()
        {
            if (!_initialized) Initialize();
            return _recipes;
        }

        /// <summary>
        /// 按类别获取配方
        /// </summary>
        public static List<GuRecipe> GetRecipesByCategory(RecipeCategory category)
        {
            if (!_initialized) Initialize();
            return _recipes.FindAll(r => r.Category == category && r.Enabled);
        }

        /// <summary>
        /// 按蛊虫等级获取配方
        /// </summary>
        public static List<GuRecipe> GetRecipesByLevel(int guLevel)
        {
            if (!_initialized) Initialize();
            return _recipes.FindAll(r => r.GuLevel == guLevel && r.Enabled);
        }

        /// <summary>
        /// 搜索配方（按名称或描述模糊匹配）
        /// </summary>
        public static List<GuRecipe> SearchRecipes(string keyword)
        {
            if (!_initialized) Initialize();
            if (string.IsNullOrWhiteSpace(keyword)) return new List<GuRecipe>(_recipes);

            keyword = keyword.ToLower();
            return _recipes.FindAll(r =>
                r.Enabled &&
                (r.ResultName.ToLower().Contains(keyword) ||
                 r.Description.ToLower().Contains(keyword))
            );
        }

        /// <summary>
        /// 获取所有可用的类别列表（有配方的类别）
        /// </summary>
        public static List<RecipeCategory> GetAvailableCategories()
        {
            if (!_initialized) Initialize();
            var categories = new List<RecipeCategory>();
            foreach (RecipeCategory cat in System.Enum.GetValues(typeof(RecipeCategory)))
            {
                if (_recipes.Exists(r => r.Category == cat && r.Enabled))
                    categories.Add(cat);
            }
            return categories;
        }

        /// <summary>
        /// 获取类别的中文名称
        /// </summary>
        public static string GetCategoryDisplayName(RecipeCategory category)
        {
            return category switch
            {
                RecipeCategory.General => "通用",
                RecipeCategory.Strength => "力道蛊",
                RecipeCategory.PigGu => "豕蛊",
                RecipeCategory.WineBug => "酒虫",
                RecipeCategory.RealmBreak => "破境丹",
                RecipeCategory.Shari => "沙砾",
                RecipeCategory.Longicorn => "天牛",
                RecipeCategory.ZiZhi => "资质",
                RecipeCategory.Consumable => "消耗品",
                _ => "其他",
            };
        }
    }
}
