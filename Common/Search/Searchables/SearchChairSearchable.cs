// ============================================================
// SearchChairSearchable - 搜索椅搜索适配器
// 将搜索椅（调试工具）包装为 ISearchable
// 搜索椅的搜索产出：调试用物品
// ============================================================
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.Search.Searchables;

/// <summary>
/// 搜索椅搜索适配器 — 调试用可搜索对象
/// </summary>
public class SearchChairSearchable : ISearchable
{
    private readonly Vector2 _worldPosition;
    private bool _exhausted;

    public SearchChairSearchable(Vector2 worldPosition)
    {
        _worldPosition = worldPosition;
    }

    public string SearchLabel => "搜索椅（调试）";

    public Vector2 WorldPosition => _worldPosition;

    /// <summary> 搜索椅难度 1（最简单，调试用） </summary>
    public int SearchDifficulty => 1;

    /// <summary> 搜索范围 </summary>
    public float SearchRange => 80f;

    /// <summary> 搜索椅每次搜索后重置（可重复搜索） </summary>
    public bool IsExhausted => _exhausted;

    /// <summary> 始终可见 </summary>
    public bool IsVisible => true;

    /// <summary>
    /// 执行搜索：生成调试用物品
    /// </summary>
    public SearchResult ExecuteSearch(Player player)
    {
        _exhausted = true;

        // 调试用：生成一些测试物品
        var loot = new List<Item>();

        // 保底元石
        var yuanS = new Item(ModContent.ItemType<Content.Items.Consumables.YuanS>());
        yuanS.stack = Main.rand.Next(1, 5);
        loot.Add(yuanS);

        // 随机额外物品
        if (Main.rand.NextFloat() < 0.3f)
        {
            var bonus = new Item(ItemID.GoldCoin);
            bonus.stack = Main.rand.Next(1, 3);
            loot.Add(bonus);
        }

        return SearchResult.CreateSuccess(loot, "从搜索椅中找到了物品");
    }

    /// <summary>
    /// 重置搜索椅（使其可再次搜索）
    /// </summary>
    public void Reset()
    {
        _exhausted = false;
    }

    public string GetPromptText()
    {
        return _exhausted ? "已搜索" : "点击搜索（调试）";
    }
}
