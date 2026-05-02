// ============================================================
// ISearchable - 可搜索对象接口
// 所有可被玩家搜索的目标（尸体、资源节点、宝箱、搜索椅等）
// 都实现此接口，由 SearchSystem 统一管理
// ============================================================
using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.Search;

/// <summary>
/// 可搜索对象接口
/// </summary>
public interface ISearchable
{
    /// <summary> 搜索目标名称（显示用，如"僵尸的尸体"、"铁矿脉"） </summary>
    string SearchLabel { get; }

    /// <summary> 搜索目标的世界坐标 </summary>
    Vector2 WorldPosition { get; }

    /// <summary> 搜索难度（1-10，影响搜索耗时和成功率） </summary>
    int SearchDifficulty { get; }

    /// <summary> 搜索范围（玩家多近可以搜索，像素单位） </summary>
    float SearchRange { get; }

    /// <summary> 搜索是否已完成/已耗尽 </summary>
    bool IsExhausted { get; }

    /// <summary> 搜索目标是否对玩家可见（靠近时是否显示提示） </summary>
    bool IsVisible { get; }

    /// <summary> 执行搜索，返回搜索结果 </summary>
    SearchResult ExecuteSearch(Player player);

    /// <summary> 获取搜索提示文本（显示在目标上方） </summary>
    string GetPromptText();
}
