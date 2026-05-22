using System.Collections.Generic;

namespace VerminLordMod.Common.Abstractions
{
    /// <summary>
    /// 仙蛊屋类型
    /// </summary>
    public enum GuHouseType
    {
        /// <summary>普通仙蛊屋 — 基础型</summary>
        Normal,
        /// <summary>战斗型仙蛊屋 — 侧重攻击</summary>
        Combat,
        /// <summary>防御型仙蛊屋 — 侧重防御</summary>
        Defense,
        /// <summary>辅助型仙蛊屋 — 治疗/增益</summary>
        Support,
        /// <summary>功能型仙蛊屋 — 移动/生产/特殊功能</summary>
        Utility,
        /// <summary>复合型仙蛊屋 — 多种功能组合</summary>
        Composite,
    }

    /// <summary>
    /// 仙蛊屋核心接口
    /// 仙蛊屋是由多只仙蛊组合而成的建筑型法宝，具有独立的等级体系和特殊能力。
    /// </summary>
    public interface IGuHouse
    {
        /// <summary>仙蛊屋等级（七转起步，最高九转）</summary>
        int HouseLevel { get; }

        /// <summary>仙蛊屋类型</summary>
        GuHouseType HouseType { get; }

        /// <summary>组成仙蛊屋的核心仙蛊列表（类型ID）</summary>
        List<int> ComponentGuTypes { get; }

        /// <summary>仙蛊屋激活所需真元</summary>
        int ActivationQiCost { get; }

        /// <summary>仙蛊屋持续运行每秒消耗真元</summary>
        int SustainQiCostPerSecond { get; }

        /// <summary>仙蛊屋覆盖范围（像素）</summary>
        float Range { get; }

        /// <summary>仙蛊屋是否可移动</summary>
        bool IsMobile { get; }
    }
}
