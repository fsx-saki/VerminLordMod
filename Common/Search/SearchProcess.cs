// ============================================================
// SearchProcess - 搜索过程状态机
// 管理搜索的进度、风险检测、中断处理
// 状态流：Idle → Searching → Success/Failed/Interrupted
// ============================================================
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace VerminLordMod.Common.Search;

/// <summary>
/// 搜索过程状态
/// </summary>
public enum SearchProcessState
{
    /// <summary> 空闲（未开始搜索） </summary>
    Idle,
    /// <summary> 搜索中（进度条增长中） </summary>
    Searching,
    /// <summary> 搜索成功 </summary>
    Success,
    /// <summary> 搜索失败 </summary>
    Failed,
    /// <summary> 搜索被中断（玩家移动/受伤/远离） </summary>
    Interrupted
}

/// <summary>
/// 搜索过程 — 管理单次搜索的完整生命周期
/// </summary>
public class SearchProcess
{
    // ===== 状态 =====
    public SearchProcessState State { get; private set; } = SearchProcessState.Idle;

    /// <summary> 搜索进度（0.0 ~ 1.0） </summary>
    public float Progress { get; private set; }

    /// <summary> 当前正在搜索的目标 </summary>
    public ISearchable? Target { get; private set; }

    /// <summary> 执行搜索的玩家 </summary>
    public Player? Searcher { get; private set; }

    // ===== 配置 =====
    /// <summary> 基础搜索耗时（秒），乘以难度系数为实际耗时 </summary>
    private const float BaseSearchTime = 1.0f;

    /// <summary> 搜索期间吸引敌人的检测间隔（帧） </summary>
    private const int AttractCheckInterval = 30;

    /// <summary> 吸引敌人概率（每帧，基于难度） </summary>
    private const float BaseAttractChance = 0.005f;

    /// <summary> 搜索中断后重置的冷却帧数 </summary>
    private const int InterruptCooldown = 60;

    // ===== 内部状态 =====
    private float _totalSearchTime;
    private int _attractTimer;
    private int _interruptCooldownTimer;
    private Vector2 _searchStartPosition;
    private int _searchStartHealth;

    /// <summary>
    /// 开始搜索
    /// </summary>
    public void Start(ISearchable target, Player player)
    {
        if (target.IsExhausted) return;

        State = SearchProcessState.Searching;
        Target = target;
        Searcher = player;
        Progress = 0f;

        // 计算总搜索耗时：基础时间 × 难度系数
        float difficultyMultiplier = 1f + (target.SearchDifficulty - 1) * 0.3f;
        _totalSearchTime = BaseSearchTime * difficultyMultiplier;

        // 记录起始状态（用于检测中断）
        _searchStartPosition = player.Center;
        _searchStartHealth = player.statLife;
        _attractTimer = 0;
        _interruptCooldownTimer = 0;
    }

    /// <summary>
    /// 每帧更新搜索过程
    /// </summary>
    /// <returns>true 表示搜索仍在进行中；false 表示搜索已结束（成功/失败/中断）</returns>
    public bool Update()
    {
        if (State != SearchProcessState.Searching)
            return false;

        if (Target == null || Searcher == null || !Searcher.active)
        {
            Interrupt("搜索目标丢失");
            return false;
        }

        // 检测中断条件
        if (CheckInterrupt())
            return false;

        // 更新进度
        float deltaTime = (float)(Main.gameTimeCache.ElapsedGameTime.TotalSeconds);
        Progress += deltaTime / _totalSearchTime;

        // 进度达到 100% → 搜索完成
        if (Progress >= 1f)
        {
            Complete();
            return false;
        }

        // 风险检测：吸引敌人
        UpdateAttractEnemies();

        return true;
    }

    /// <summary>
    /// 检测搜索是否应被中断
    /// </summary>
    private bool CheckInterrupt()
    {
        if (Searcher == null) return false;

        // 1. 玩家移动过多（离开起始位置超过 3 格 = 48px）
        float moveDist = Vector2.Distance(Searcher.Center, _searchStartPosition);
        if (moveDist > 48f)
        {
            Interrupt("移动打断了搜索");
            return true;
        }

        // 2. 玩家受伤（生命值减少）
        if (Searcher.statLife < _searchStartHealth)
        {
            Interrupt("受伤打断了搜索");
            return true;
        }

        // 3. 玩家远离目标
        if (Target != null)
        {
            float distToTarget = Vector2.Distance(Searcher.Center, Target.WorldPosition);
            if (distToTarget > Target.SearchRange * 1.5f)
            {
                Interrupt("距离太远，搜索中断");
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 更新吸引敌人逻辑
    /// </summary>
    private void UpdateAttractEnemies()
    {
        if (Target == null || Searcher == null) return;

        _attractTimer++;
        if (_attractTimer < AttractCheckInterval) return;
        _attractTimer = 0;

        // 搜索难度越高，吸引敌人的概率越大
        float attractChance = BaseAttractChance * (1f + Target.SearchDifficulty * 0.5f);

        if (Main.rand.NextFloat() < attractChance)
        {
            // 查找附近的敌人，吸引它们
            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.townNPC) continue;
                if (npc.life <= 0) continue;

                float dist = Vector2.Distance(npc.Center, Searcher.Center);
                if (dist < Target.SearchRange * 3f)
                {
                    // 让敌人注意到玩家位置
                    npc.target = Searcher.whoAmI;
                    npc.velocity = Vector2.Normalize(Searcher.Center - npc.Center) * npc.velocity.Length();
                }
            }
        }
    }

    /// <summary>
    /// 搜索完成（进度达到 100%）
    /// </summary>
    private void Complete()
    {
        if (Target == null || Searcher == null)
        {
            State = SearchProcessState.Failed;
            return;
        }

        // 执行搜索，获取结果
        var result = Target.ExecuteSearch(Searcher);

        if (result.Success)
        {
            State = SearchProcessState.Success;
        }
        else
        {
            State = SearchProcessState.Failed;
        }

        // 如果吸引了敌人，标记
        if (result.AttractedEnemies)
        {
            // 敌人已在 UpdateAttractEnemies 中被吸引
        }
    }

    /// <summary>
    /// 中断搜索
    /// </summary>
    public void Interrupt(string reason = "")
    {
        State = SearchProcessState.Interrupted;
        _interruptCooldownTimer = InterruptCooldown;
    }

    /// <summary>
    /// 重置搜索过程
    /// </summary>
    public void Reset()
    {
        State = SearchProcessState.Idle;
        Progress = 0f;
        Target = null;
        Searcher = null;
        _attractTimer = 0;
    }

    /// <summary>
    /// 获取搜索结果的摘要文本
    /// </summary>
    public string GetStatusText()
    {
        return State switch
        {
            SearchProcessState.Idle => "等待搜索",
            SearchProcessState.Searching => $"搜索中... {(int)(Progress * 100)}%",
            SearchProcessState.Success => "搜索完成！",
            SearchProcessState.Failed => "搜索失败",
            SearchProcessState.Interrupted => "搜索已中断",
            _ => "未知状态"
        };
    }
}
