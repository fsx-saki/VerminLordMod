// ============================================================
// CorpseSearchable - 尸体搜索适配器
// 将 NpcCorpse 包装为 ISearchable，使尸体可以通过搜索系统统一管理
// ============================================================
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using VerminLordMod.Common.Entities;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Search.Searchables;

/// <summary>
/// 尸体搜索适配器 — 包装 NpcCorpse 为 ISearchable
/// </summary>
public class CorpseSearchable : ISearchable
{
    private readonly NpcCorpse _corpse;

    public CorpseSearchable(NpcCorpse corpse)
    {
        _corpse = corpse;
    }

    /// <summary> 尸体名称 </summary>
    public string SearchLabel => _corpse.OwnerName + "的尸体";

    /// <summary> 尸体世界坐标 </summary>
    public Vector2 WorldPosition => _corpse.Projectile.Center;

    /// <summary>
    /// 搜索难度基于尸体类型
    /// - 玩家尸体：难度 3
    /// - 怪物尸体：难度 2
    /// - Boss 尸体：难度 4
    /// </summary>
    public int SearchDifficulty => _corpse.CorpseType switch
    {
        CorpseType.Player => 3,
        CorpseType.Boss => 4,
        _ => 2
    };

    /// <summary> 搜索范围（与 LootSystem.PlayerDetectionRange 一致） </summary>
    public float SearchRange => LootSystem.PlayerDetectionRange;

    /// <summary> 尸体是否已耗尽（无剩余物品或已被 NPC 搜过） </summary>
    public bool IsExhausted =>
        !_corpse.Projectile.active ||
        _corpse.IsLootedByNPC ||
        (!_corpse.HasRemainingItems() && _corpse.HasBeenSearchedByPlayer);

    /// <summary> 尸体是否可见（未被搜过且未被 NPC 搜过） </summary>
    public bool IsVisible =>
        _corpse.Projectile.active &&
        !_corpse.IsLootedByNPC &&
        !_corpse.HasBeenSearchedByPlayer;

    /// <summary>
    /// 执行搜索：从尸体中获取所有剩余物品
    /// </summary>
    public SearchResult ExecuteSearch(Player player)
    {
        if (!_corpse.Projectile.active || !_corpse.HasRemainingItems())
        {
            return SearchResult.CreateFailure("尸体已被搜刮干净");
        }

        // 标记尸体已被搜索
        _corpse.HasBeenSearchedByPlayer = true;

        // 获取所有剩余物品
        var loot = new List<Item>(_corpse.RemainingItems);
        _corpse.RemainingItems.Clear();

        return SearchResult.CreateSuccess(loot, $"从 {_corpse.OwnerName} 的尸体中找到了物品");
    }

    /// <summary>
    /// 获取搜索提示文本
    /// </summary>
    public string GetPromptText()
    {
        if (_corpse.IsLootedByNPC)
            return "已被搜刮";

        if (!_corpse.HasRemainingItems())
            return "空尸体";

        int count = 0;
        foreach (var item in _corpse.RemainingItems)
        {
            if (item != null && !item.IsAir)
                count++;
        }

        return $"点击搜索 ({count} 件物品)";
    }
}
