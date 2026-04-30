// ============================================================
// SimpleUIToggle - 触发 SimpleUI 面板的物品
// 左键使用：展开/收起淡紫色页面
// 不依赖任何现有UI系统
// ============================================================
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.UI.SimpleUI;

namespace VerminLordMod.Content.Items.Debuggers;

/// <summary>
/// SimpleUI 面板触发物品
/// 左键使用切换面板展开/收起
/// </summary>
public class SimpleUIToggle : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.rare = ItemRarityID.Master;
        Item.maxStack = 1;
        Item.value = 100;

        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Guitar;
        Item.autoReuse = false;
        Item.useTurn = true;
        Item.UseSound = SoundID.Item1;
    }

    public override bool? UseItem(Player player)
    {
        // 切换 SimpleUI 面板
        SimpleUISystem.TogglePanel();
        return true;
    }

    public override bool AltFunctionUse(Player player) => false;

    public override void AddRecipes()
    {
        // 调试物品，无合成配方
    }
}
