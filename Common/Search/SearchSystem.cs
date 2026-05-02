// ============================================================
// SearchSystem - 搜索系统核心（ModSystem）
// 管理所有 ISearchable 实例的注册、玩家附近检测、搜索过程生命周期
// 通过 SimpleUISystem 注册 UI 绘制层
// ============================================================
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI.SimpleUI;

namespace VerminLordMod.Common.Search;

/// <summary>
/// 搜索系统 — 核心管理器
/// </summary>
[Autoload(Side = ModSide.Client)]
public class SearchSystem : ModSystem
{
    // ===== 单例 =====
    public static SearchSystem Instance => ModContent.GetInstance<SearchSystem>();

    // ===== 可搜索对象注册表 =====
    private readonly List<ISearchable> _searchables = new();

    /// <summary> 当前正在进行的搜索过程 </summary>
    public SearchProcess CurrentProcess { get; } = new();

    /// <summary> 玩家附近的可搜索目标（每帧更新） </summary>
    public ISearchable? NearbyTarget { get; private set; }

    /// <summary> 是否正在搜索中 </summary>
    public bool IsSearching => CurrentProcess.State == SearchProcessState.Searching;

    /// <summary> 是否显示了搜索结果 </summary>
    public bool HasResult =>
        CurrentProcess.State == SearchProcessState.Success ||
        CurrentProcess.State == SearchProcessState.Failed;

    // ============================================================
    // 注册/注销
    // ============================================================

    /// <summary>
    /// 注册一个可搜索对象
    /// </summary>
    public void Register(ISearchable searchable)
    {
        if (!_searchables.Contains(searchable))
            _searchables.Add(searchable);
    }

    /// <summary>
    /// 注销一个可搜索对象
    /// </summary>
    public void Unregister(ISearchable searchable)
    {
        _searchables.Remove(searchable);
    }

    /// <summary>
    /// 查找指定类型的可搜索对象（用于外部系统检查是否已注册）
    /// </summary>
    public T? Find<T>(System.Predicate<T> predicate) where T : class, ISearchable
    {
        foreach (var s in _searchables)
        {
            if (s is T typed && predicate(typed))
                return typed;
        }
        return null;
    }

    // ============================================================
    // 搜索操作 API
    // ============================================================

    /// <summary>
    /// 玩家开始搜索当前附近的目标
    /// </summary>
    public bool StartSearch(Player player)
    {
        if (NearbyTarget == null || NearbyTarget.IsExhausted)
            return false;

        if (IsSearching)
            return false;

        CurrentProcess.Start(NearbyTarget, player);
        return true;
    }

    /// <summary>
    /// 取消当前搜索
    /// </summary>
    public void CancelSearch()
    {
        CurrentProcess.Interrupt("玩家取消了搜索");
    }

    /// <summary>
    /// 获取当前搜索结果（搜索完成后调用）
    /// </summary>
    public SearchResult? GetResult()
    {
        if (!HasResult) return null;

        // 重新执行搜索获取结果
        if (CurrentProcess.Target != null && CurrentProcess.Searcher != null)
        {
            return CurrentProcess.Target.ExecuteSearch(CurrentProcess.Searcher);
        }
        return null;
    }

    // ============================================================
    // ModSystem 生命周期
    // ============================================================

    public override void Load()
    {
        // 注册到 SimpleUISystem 的绘制层
    }

    public override void Unload()
    {
        _searchables.Clear();
    }

    public override void UpdateUI(GameTime gameTime)
    {
        Player player = Main.LocalPlayer;
        if (player == null || !player.active || Main.dedServ)
            return;

        // 1. 检测玩家附近的可搜索目标
        UpdateNearbyDetection(player);

        // 2. 更新搜索过程
        if (IsSearching)
        {
            bool stillSearching = CurrentProcess.Update();

            if (!stillSearching)
            {
                // 搜索结束，处理结果
                HandleSearchResult(player);
            }
        }
    }

    // ============================================================
    // 内部逻辑
    // ============================================================

    /// <summary>
    /// 检测玩家附近的可搜索目标
    /// </summary>
    private void UpdateNearbyDetection(Player player)
    {
        ISearchable? closest = null;
        float closestDist = float.MaxValue;

        foreach (var searchable in _searchables)
        {
            if (searchable.IsExhausted || !searchable.IsVisible)
                continue;

            float dist = Vector2.Distance(player.Center, searchable.WorldPosition);
            if (dist < searchable.SearchRange && dist < closestDist)
            {
                closest = searchable;
                closestDist = dist;
            }
        }

        NearbyTarget = closest;
    }

    /// <summary>
    /// 处理搜索结果
    /// </summary>
    private void HandleSearchResult(Player player)
    {
        var searchPlayer = player.GetModPlayer<SearchPlayer>();
        var target = CurrentProcess.Target;

        if (target == null) return;

        int difficulty = target.SearchDifficulty;

        if (CurrentProcess.State == SearchProcessState.Success)
        {
            searchPlayer.OnSearchSuccess(difficulty);
            Main.NewText($"搜索完成！从 {target.SearchLabel} 中获得了物品。", Color.Green);
        }
        else if (CurrentProcess.State == SearchProcessState.Failed)
        {
            searchPlayer.OnSearchFailure(difficulty);
            Main.NewText($"搜索 {target.SearchLabel} 失败。", Color.Gray);
        }
        else if (CurrentProcess.State == SearchProcessState.Interrupted)
        {
            // 中断不记录统计
        }
    }

    // ============================================================
    // 绘制
    // ============================================================

    /// <summary>
    /// 绘制搜索 UI（由 SimpleUISystem 调用）
    /// </summary>
    public void DrawUI(SpriteBatch sb)
    {
        // 由 SearchUI 组件负责绘制
        // SearchPromptUI / SearchProgressUI / SearchResultUI
    }
}
