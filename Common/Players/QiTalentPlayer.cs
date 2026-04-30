using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 资质系统（从 QiPlayer 拆分）
    /// 职责：资质存储、旧值映射、修炼效率加成
    /// 不处理：真元存储、境界计算、战斗数值修改
    /// </summary>
    public class QiTalentPlayer : ModPlayer
    {
        /// <summary>
        /// 资质等级枚举（与旧 ZiZhi 枚举对应）
        /// </summary>
        public enum TalentGrade
        {
            /// <summary>丁等（旧值 2）</summary>
            Ding = 0,
            /// <summary>丙等（旧值 4）</summary>
            Bing = 1,
            /// <summary>乙等（旧值 6）</summary>
            Yi = 2,
            /// <summary>甲等（旧值 8）</summary>
            Jia = 3
        }

        /// <summary>当前资质等级</summary>
        public TalentGrade Grade;

        /// <summary>修炼效率倍率</summary>
        public float CultivationSpeedMultiplier;

        /// <summary>感知范围加成</summary>
        public float PerceptionRangeBonus;

        // 旧代码 ZiZhi 数值映射：丁=2, 丙=4, 乙=6, 甲=8
        private static readonly float[] ZiZhiLegacyValues = { 2f, 4f, 6f, 8f };
        private static readonly float[] GradeMultipliers = { 0.8f, 1.0f, 1.3f, 1.8f };
        private static readonly float[] PerceptionBonuses = { 0f, 10f, 25f, 50f };

        /// <summary>
        /// 返回与旧 (int)ZiZhi 等价的数值，供 QiRealmPlayer 的旧公式使用。
        /// </summary>
        public float GetZiZhiMultiplier() => ZiZhiLegacyValues[(int)Grade];

        /// <summary>
        /// 开窍初始化。MVA 默认乙等，后续可随机或玩家选择。
        /// </summary>
        public void OnAwakening(TalentGrade? fixedGrade = null)
        {
            Grade = fixedGrade ?? TalentGrade.Yi;
            CalculateEffects();
        }

        /// <summary>
        /// 根据当前资质重新计算效果。
        /// </summary>
        public void CalculateEffects()
        {
            int idx = (int)Grade;
            CultivationSpeedMultiplier = GradeMultipliers[idx];
            PerceptionRangeBonus = PerceptionBonuses[idx];
        }

        /// <summary>
        /// 从旧 ZiZhi 枚举值转换到新 TalentGrade。
        /// </summary>
        public static TalentGrade FromLegacyZiZhi(int legacyZiZhi)
        {
            return legacyZiZhi switch
            {
                10 => TalentGrade.Jia,   // GUA=10
                8 => TalentGrade.Jia,    // RJIA=8
                6 => TalentGrade.Yi,     // RYI=6
                4 => TalentGrade.Bing,   // RBING=4
                2 => TalentGrade.Ding,   // RDING=2
                0 => TalentGrade.Ding,   // RO=0
                _ => TalentGrade.Yi      // 默认乙等
            };
        }

        /// <summary>
        /// 重置资质（用于调试）。
        /// </summary>
        public void ResetAll()
        {
            Grade = TalentGrade.Ding;
            CalculateEffects();
        }

        // ===== 数据持久化 =====
        public override void SaveData(TagCompound tag)
        {
            tag["TalentGrade"] = (int)Grade;
        }

        public override void LoadData(TagCompound tag)
        {
            Grade = (TalentGrade)tag.GetInt("TalentGrade");
            CalculateEffects();
        }
    }
}
