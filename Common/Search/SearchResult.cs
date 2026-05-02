// ============================================================
// SearchResult - 搜索结果数据结构
// 包含搜索成功/失败、获得的物品、触发的风险等信息
// ============================================================
using System.Collections.Generic;
using Terraria;

namespace VerminLordMod.Common.Search;

/// <summary>
/// 搜索结果
/// </summary>
public class SearchResult
{
    /// <summary> 搜索是否成功 </summary>
    public bool Success { get; set; }

    /// <summary> 获得的物品列表 </summary>
    public List<Item> Loot { get; set; } = new();

    /// <summary> 发现的线索/信息文本 </summary>
    public string Discovery { get; set; } = string.Empty;

    /// <summary> 是否触发了陷阱 </summary>
    public bool TriggeredTrap { get; set; }

    /// <summary> 是否吸引了敌人 </summary>
    public bool AttractedEnemies { get; set; }

    /// <summary> 吸引的敌人 whoAmI 列表 </summary>
    public List<int> AttractedNPCWhoAmI { get; set; } = new();

    /// <summary> 是否有物品可拾取 </summary>
    public bool HasLoot => Loot.Count > 0;

    /// <summary>
    /// 创建一个成功的搜索结果
    /// </summary>
    public static SearchResult CreateSuccess(List<Item>? loot = null, string discovery = "")
    {
        return new SearchResult
        {
            Success = true,
            Loot = loot ?? new List<Item>(),
            Discovery = discovery
        };
    }

    /// <summary>
    /// 创建一个失败的搜索结果
    /// </summary>
    public static SearchResult CreateFailure(string reason = "", bool triggerTrap = false)
    {
        return new SearchResult
        {
            Success = false,
            Discovery = reason,
            TriggeredTrap = triggerTrap
        };
    }
}
