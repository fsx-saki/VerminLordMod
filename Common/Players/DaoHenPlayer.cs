using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.GuBehaviors;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 道痕系统玩家组件
    /// 职责：道痕积累、倍率计算、道痕标记判定
    /// 
    /// 道痕是蛊师修炼过程中领悟的法则印记，按道途分类（如搬道、御道等）。
    /// 积累到一定阈值后提供对应道途的倍率加成：
    /// - 50+ → 1.1x
    /// - 200+ → 1.25x
    /// - 500+ → 1.5x
    /// </summary>
    public class DaoHenPlayer : ModPlayer
    {
        /// <summary>本地玩家单例（仅供客户端使用）</summary>
        public static DaoHenPlayer Instance => Main.LocalPlayer.GetModPlayer<DaoHenPlayer>();

        /// <summary>各道途的道痕积累值，键为 DaoType 枚举</summary>
        public Dictionary<DaoType, float> DaoHen = new Dictionary<DaoType, float>();

        /// <summary>初始化所有道途的道痕值为0</summary>
        public override void Initialize()
        {
            foreach (DaoType dao in System.Enum.GetValues(typeof(DaoType)))
                DaoHen[dao] = 0f;
        }

        /// <summary>
        /// 获取指定道途的倍率加成。
        /// 阈值：50→1.1x, 200→1.25x, 500→1.5x, 不足→1.0x
        /// </summary>
        public float GetMultiplier(DaoType dao)
        {
            if (DaoHen.TryGetValue(dao, out float val))
            {
                if (val >= 500) return 1.5f;
                if (val >= 200) return 1.25f;
                if (val >= 50) return 1.1f;
            }
            return 1f;
        }

        /// <summary>为指定道途增加道痕值</summary>
        public void AddDaoHen(DaoType dao, float amount)
        {
            if (DaoHen.ContainsKey(dao))
                DaoHen[dao] += amount;
        }

        /// <summary>是否拥有任何道痕标记（任一道途 ≥ 50）</summary>
        public bool HasDaoMark()
        {
            foreach (var kvp in DaoHen)
            {
                if (kvp.Value >= 50f)
                    return true;
            }
            return false;
        }

        /// <summary>获取所有道途的道痕总值</summary>
        public float GetTotalDaoHen()
        {
            float total = 0f;
            foreach (var kvp in DaoHen)
                total += kvp.Value;
            return total;
        }
    }
}
