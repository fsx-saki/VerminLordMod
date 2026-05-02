// ============================================================
// SearchPlayer - 玩家搜索技能组件（ModPlayer）
// 管理玩家的搜索技能属性、统计数据和持久化
// ============================================================
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Players;

/// <summary>
/// 玩家搜索技能组件
/// </summary>
public class SearchPlayer : ModPlayer
{
    // ===== 搜索技能属性 =====

    /// <summary> 搜索速度（减少搜索耗时，0-100） </summary>
    public float SearchSpeed { get; set; }

    /// <summary> 搜索精度（提高成功率、发现稀有物品概率，0-100） </summary>
    public float SearchPrecision { get; set; }

    /// <summary> 搜索警觉（降低被偷袭概率、提前发现敌人，0-100） </summary>
    public float SearchAwareness { get; set; }

    /// <summary> 搜索知识（解锁特殊搜索目标、发现隐藏内容，0-100） </summary>
    public float SearchKnowledge { get; set; }

    // ===== 统计数据 =====

    /// <summary> 总搜索次数 </summary>
    public int TotalSearches { get; set; }

    /// <summary> 成功搜索次数 </summary>
    public int SuccessfulSearches { get; set; }

    /// <summary> 触发陷阱次数 </summary>
    public int TrapTriggers { get; set; }

    /// <summary> 搜索吸引的敌人数量 </summary>
    public int AttractedEnemies { get; set; }

    // ===== 计算属性 =====

    /// <summary> 搜索成功率加成（0.0 ~ 1.0） </summary>
    public float SuccessRateBonus => SearchPrecision * 0.005f;

    /// <summary> 搜索速度倍率（最小 0.3，即最快 70% 加速） </summary>
    public float SpeedMultiplier => 1.0f - SearchSpeed * 0.007f;

    /// <summary> 警觉范围加成（像素） </summary>
    public float AwarenessRangeBonus => SearchAwareness * 1.5f;

    /// <summary> 稀有物品发现概率加成 </summary>
    public float RareFindBonus => SearchPrecision * 0.003f + SearchKnowledge * 0.005f;

    /// <summary> 搜索成功率 </summary>
    public float SuccessRate => TotalSearches > 0
        ? (float)SuccessfulSearches / TotalSearches
        : 0f;

    // ============================================================
    // 技能提升
    // ============================================================

    /// <summary>
    /// 搜索成功后提升技能
    /// </summary>
    public void OnSearchSuccess(int difficulty)
    {
        SuccessfulSearches++;
        TotalSearches++;

        // 根据难度获得技能经验
        float expGain = 0.5f + difficulty * 0.3f;

        // 主要提升精度（搜索成功主要靠精度）
        SearchPrecision = ClampSkill(SearchPrecision + expGain * 0.6f);

        // 次要提升速度
        SearchSpeed = ClampSkill(SearchSpeed + expGain * 0.3f);
    }

    /// <summary>
    /// 搜索失败后提升技能
    /// </summary>
    public void OnSearchFailure(int difficulty)
    {
        TotalSearches++;

        // 失败也能学到东西，但较少
        float expGain = 0.3f + difficulty * 0.2f;

        // 失败主要提升警觉（吃一堑长一智）
        SearchAwareness = ClampSkill(SearchAwareness + expGain * 0.5f);
        SearchKnowledge = ClampSkill(SearchKnowledge + expGain * 0.3f);
    }

    /// <summary>
    /// 触发陷阱时提升警觉
    /// </summary>
    public void OnTrapTriggered()
    {
        TrapTriggers++;
        SearchAwareness = ClampSkill(SearchAwareness + 1.0f);
    }

    /// <summary>
    /// 吸引敌人时提升警觉
    /// </summary>
    public void OnEnemyAttracted(int count)
    {
        AttractedEnemies += count;
        SearchAwareness = ClampSkill(SearchAwareness + 0.5f * count);
    }

    /// <summary>
    /// 将技能值限制在 0-100 范围内
    /// </summary>
    private static float ClampSkill(float value)
    {
        return System.Math.Clamp(value, 0f, 100f);
    }

    // ============================================================
    // 数据持久化
    // ============================================================

    public override void SaveData(TagCompound tag)
    {
        tag["searchSpeed"] = SearchSpeed;
        tag["searchPrecision"] = SearchPrecision;
        tag["searchAwareness"] = SearchAwareness;
        tag["searchKnowledge"] = SearchKnowledge;
        tag["totalSearches"] = TotalSearches;
        tag["successfulSearches"] = SuccessfulSearches;
        tag["trapTriggers"] = TrapTriggers;
        tag["attractedEnemies"] = AttractedEnemies;
    }

    public override void LoadData(TagCompound tag)
    {
        SearchSpeed = tag.GetFloat("searchSpeed");
        SearchPrecision = tag.GetFloat("searchPrecision");
        SearchAwareness = tag.GetFloat("searchAwareness");
        SearchKnowledge = tag.GetFloat("searchKnowledge");
        TotalSearches = tag.GetInt("totalSearches");
        SuccessfulSearches = tag.GetInt("successfulSearches");
        TrapTriggers = tag.GetInt("trapTriggers");
        AttractedEnemies = tag.GetInt("attractedEnemies");
    }

    /// <summary>
    /// 获取技能摘要文本
    /// </summary>
    public string GetSkillSummary()
    {
        return $"搜索技能: 速度 {SearchSpeed:F0}/100 | 精度 {SearchPrecision:F0}/100 | " +
               $"警觉 {SearchAwareness:F0}/100 | 知识 {SearchKnowledge:F0}/100 | " +
               $"总搜索 {TotalSearches} 次 | 成功率 {SuccessRate * 100:F1}%";
    }
}
