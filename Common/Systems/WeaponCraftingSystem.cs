using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Systems
{
    // ============================================================
    // WeaponCraftingSystem — 炼器/重铸系统大框
    //
    // 系统定位：
    // 炼器是将蛊虫武器强化/重铸/精炼的系统。
    // 对应小说中的炼器之道，蛊师可以提升武器的控制率、品质、属性。
    //
    // 功能规划：
    // 1. 精炼强化：消耗元石提升武器控制率（GuWeaponItem.controlRate已有）
    // 2. 重铸改造：改变武器的属性分配（偏攻/偏防/偏速）
    // 3. 道域印记：给武器附加道痕标签
    // 4. 蛊虫合炼：将多只蛊虫合炼为更强的组合武器
    // 5. 炼器炉Tile：需要站在炼器炉旁才能炼器
    // 6. 炼器UI：选择炼器方式、投入材料、确认操作
    // 7. 炼器师NPC：御堂家老提供炼器指导
    //
    // TODO:
    //   - 实现精炼强化逻辑
    //   - 实现重铸改造逻辑
    //   - 实现道域印记附加
    //   - 实现蛊虫合炼
    //   - 创建炼器炉Tile
    //   - 实现炼器UI
    // ============================================================

    public enum RefineType
    {
        ControlRateUp,      // 精炼：提升控制率
        DamageUp,           // 强化：提升伤害
        DefenseUp,          // 稳固：提升防御
        SpeedUp,            // 轻灵：提升攻速
        DaoHenImprint,      // 印记：附加道痕标签
        AttributeShift,     // 转化：改变属性倾向
        CombineGu           // 合炼：组合多蛊
    }

    public class RefineRecipe
    {
        public RefineType Type;
        public int RequiredYuanStones;
        public int RequiredQiCost;
        public int RequiredGuLevel;
        public float SuccessRate;
        public List<int> RequiredMaterials = new();
    }

    public class WeaponCraftingSystem : ModSystem
    {
        public static WeaponCraftingSystem Instance => ModContent.GetInstance<WeaponCraftingSystem>();

        public Dictionary<RefineType, RefineRecipe> RefineRecipes = new();

        public override void OnWorldLoad()
        {
            RefineRecipes.Clear();
            RegisterRefineRecipes();
        }

        private void RegisterRefineRecipes()
        {
            // TODO: 注册炼器配方
            RefineRecipes[RefineType.ControlRateUp] = new RefineRecipe
            {
                Type = RefineType.ControlRateUp,
                RequiredYuanStones = 10,
                RequiredQiCost = 20,
                RequiredGuLevel = 1,
                SuccessRate = 0.8f
            };
            // etc.
        }

        public bool AttemptRefine(Player player, Item weapon, RefineType type)
        {
            if (!RefineRecipes.TryGetValue(type, out var recipe)) return false;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            if (qiRealm.GuLevel < recipe.RequiredGuLevel) return false;

            // TODO: 检查元石消耗
            // TODO: 检查真元消耗

            float successRate = recipe.SuccessRate + qiRealm.GuLevel * 0.03f;
            bool success = Main.rand.NextFloat() <= successRate;

            if (success)
            {
                ApplyRefineEffect(weapon, type);
                Main.NewText("炼器成功！", Microsoft.Xna.Framework.Color.Green);
            }
            else
            {
                Main.NewText("炼器失败，材料损失！", Microsoft.Xna.Framework.Color.Red);
            }

            return success;
        }

        private void ApplyRefineEffect(Item weapon, RefineType type)
        {
            // TODO: 实现炼器效果
            // if (weapon.ModItem is GuWeaponItem guWeapon)
            // {
            //     switch (type)
            //     {
            //         case RefineType.ControlRateUp:
            //             guWeapon.controlRate += 5f;
            //             break;
            //         case RefineType.DamageUp:
            //             weapon.damage += 2;
            //             break;
            //         // etc.
            //     }
            // }
        }

        // ===== 重铸保留（已有 RefinementGlobalItem） =====
        // RefinementGlobalItem 已实现重铸时保持 controlRate
        // 此系统扩展更丰富的炼器功能
    }
}