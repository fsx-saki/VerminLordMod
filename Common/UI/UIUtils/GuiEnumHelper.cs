// ============================================================
// GuiEnumHelper - 枚举值中文名称/颜色辅助类
// 集中管理所有枚举值的中文显示和颜色映射
// 消除各文件中重复的 switch 语句
// ============================================================
using Microsoft.Xna.Framework;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.UI.UIUtils;

/// <summary>
/// 枚举值中文名称/颜色辅助类
/// </summary>
public static class GuiEnumHelper
{
    // ============================================================
    // GuAttitude
    // ============================================================

    /// <summary>
    /// 获取态度中文名称
    /// </summary>
    public static string GetAttitudeName(GuAttitude attitude)
    {
        return attitude switch
        {
            GuAttitude.Ignore => "无视",
            GuAttitude.Friendly => "友好",
            GuAttitude.Wary => "警惕",
            GuAttitude.Hostile => "敌对",
            GuAttitude.Fearful => "恐惧",
            GuAttitude.Respectful => "尊敬",
            GuAttitude.Contemptuous => "轻蔑",
            _ => "未知"
        };
    }

    /// <summary>
    /// 获取态度显示颜色
    /// </summary>
    public static Color GetAttitudeColor(GuAttitude attitude)
    {
        return attitude switch
        {
            GuAttitude.Ignore => Color.Gray,
            GuAttitude.Friendly => Color.Green,
            GuAttitude.Wary => Color.Yellow,
            GuAttitude.Hostile => Color.Red,
            GuAttitude.Fearful => Color.Purple,
            GuAttitude.Respectful => Color.Gold,
            GuAttitude.Contemptuous => Color.Orange,
            _ => Color.White
        };
    }

    /// <summary>
    /// 获取态度描述
    /// </summary>
    public static string GetAttitudeDescription(GuAttitude attitude)
    {
        return attitude switch
        {
            GuAttitude.Ignore => "完全无视你的存在",
            GuAttitude.Friendly => "对你友好相待",
            GuAttitude.Wary => "对你保持警惕",
            GuAttitude.Hostile => "对你充满敌意",
            GuAttitude.Fearful => "对你感到恐惧",
            GuAttitude.Respectful => "对你表示尊敬",
            GuAttitude.Contemptuous => "对你表示轻蔑",
            _ => ""
        };
    }

    // ============================================================
    // GuPersonality
    // ============================================================

    /// <summary>
    /// 获取性格中文名称
    /// </summary>
    public static string GetPersonalityName(GuPersonality personality)
    {
        return personality switch
        {
            GuPersonality.Neutral => "中立",
            GuPersonality.Aggressive => "好斗",
            GuPersonality.Cautious => "谨慎",
            GuPersonality.Greedy => "贪婪",
            GuPersonality.Loyal => "忠诚",
            GuPersonality.Treacherous => "反复无常",
            GuPersonality.Benevolent => "仁慈",
            GuPersonality.Proud => "高傲",
            _ => "未知"
        };
    }

    /// <summary>
    /// 获取性格显示颜色
    /// </summary>
    public static Color GetPersonalityColor(GuPersonality personality)
    {
        return personality switch
        {
            GuPersonality.Neutral => Color.White,
            GuPersonality.Aggressive => Color.Red,
            GuPersonality.Cautious => Color.LightBlue,
            GuPersonality.Greedy => Color.Gold,
            GuPersonality.Loyal => Color.Green,
            GuPersonality.Treacherous => Color.Purple,
            GuPersonality.Benevolent => Color.LightGreen,
            GuPersonality.Proud => Color.Orange,
            _ => Color.White
        };
    }

    /// <summary>
    /// 获取性格描述
    /// </summary>
    public static string GetPersonalityDescription(GuPersonality personality)
    {
        return personality switch
        {
            GuPersonality.Neutral => "行事中立，不偏不倚",
            GuPersonality.Aggressive => "性格好斗，容易主动攻击",
            GuPersonality.Cautious => "行事谨慎，打不过就跑",
            GuPersonality.Greedy => "生性贪婪，对宝物敏感",
            GuPersonality.Loyal => "忠诚可靠，对家族忠心",
            GuPersonality.Treacherous => "反复无常，可能背刺",
            GuPersonality.Benevolent => "心地仁慈，不会下杀手",
            GuPersonality.Proud => "生性高傲，被挑衅必反击",
            _ => ""
        };
    }

    // ============================================================
    // GuRank
    // ============================================================

    /// <summary>
    /// 获取境界的中文名称
    /// </summary>
    public static string GetRankName(GuRank rank)
    {
        return rank switch
        {
            GuRank.Zhuan1_Chu => "一转初阶",
            GuRank.Zhuan1_Zhong => "一转中阶",
            GuRank.Zhuan1_Gao => "一转高阶",
            GuRank.Zhuan1_DianFeng => "一转巅峰",
            GuRank.Zhuan2_Chu => "二转初阶",
            GuRank.Zhuan2_Zhong => "二转中阶",
            GuRank.Zhuan2_Gao => "二转高阶",
            GuRank.Zhuan2_DianFeng => "二转巅峰",
            GuRank.Zhuan3_Chu => "三转初阶",
            GuRank.Zhuan3_Zhong => "三转中阶",
            GuRank.Zhuan3_Gao => "三转高阶",
            GuRank.Zhuan3_DianFeng => "三转巅峰",
            GuRank.Zhuan4_Chu => "四转初阶",
            GuRank.Zhuan4_Zhong => "四转中阶",
            GuRank.Zhuan4_Gao => "四转高阶",
            GuRank.Zhuan4_DianFeng => "四转巅峰",
            GuRank.Zhuan5_Chu => "五转初阶",
            GuRank.Zhuan5_Zhong => "五转中阶",
            GuRank.Zhuan5_Gao => "五转高阶",
            GuRank.Zhuan5_DianFeng => "五转巅峰",
            _ => "未知"
        };
    }

    /// <summary>
    /// 获取境界的显示颜色
    /// </summary>
    public static Color GetRankColor(GuRank rank)
    {
        return rank switch
        {
            GuRank.Zhuan1_Chu or GuRank.Zhuan1_Zhong or GuRank.Zhuan1_Gao or GuRank.Zhuan1_DianFeng => Color.Gray,
            GuRank.Zhuan2_Chu or GuRank.Zhuan2_Zhong or GuRank.Zhuan2_Gao or GuRank.Zhuan2_DianFeng => Color.LightBlue,
            GuRank.Zhuan3_Chu or GuRank.Zhuan3_Zhong or GuRank.Zhuan3_Gao or GuRank.Zhuan3_DianFeng => Color.Green,
            GuRank.Zhuan4_Chu or GuRank.Zhuan4_Zhong or GuRank.Zhuan4_Gao or GuRank.Zhuan4_DianFeng => Color.Gold,
            GuRank.Zhuan5_Chu or GuRank.Zhuan5_Zhong or GuRank.Zhuan5_Gao or GuRank.Zhuan5_DianFeng => Color.Purple,
            _ => Color.White
        };
    }

    // ============================================================
    // RepLevel
    // ============================================================

    /// <summary>
    /// 获取声望等级的中文名称
    /// </summary>
    public static string GetRepLevelName(RepLevel level)
    {
        return level switch
        {
            RepLevel.Hostile => "敌对",
            RepLevel.Unfriendly => "冷淡",
            RepLevel.Neutral => "中立",
            RepLevel.Friendly => "友好",
            RepLevel.Allied => "同盟",
            _ => "未知"
        };
    }
}
