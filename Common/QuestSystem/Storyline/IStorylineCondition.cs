using Terraria;

namespace VerminLordMod.Common.QuestSystem.Storyline
{
    /// <summary>
    /// 主线触发条件接口。
    /// 仿照 PRT 系统"模板 + 参数替换"哲学，
    /// 所有条件只需实现此接口即可即插即用。
    /// </summary>
    public interface IStorylineCondition
    {
        /// <summary>条件描述（用于日志和调试）</summary>
        string Description { get; }

        /// <summary>每 60 tick 检查一次，返回 true 表示条件满足</summary>
        bool IsMet(Player player);
    }
}
