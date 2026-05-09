using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// D-09: 统一玩家死亡处理链。
    ///
    /// 钩住玩家死亡事件，按顺序执行死亡处理链：
    ///   1. 发布 PlayerDyingEvent（供外部系统监听，如 ChunQiuChanPlayer 设置 IsRebirthProtected）
    ///   2. KongQiaoPlayer.OnPlayerDeath() — 蛊虫逃跑/自毁/保留
    ///   3. QiResourcePlayer.OnDeathClearQi() — 真元清空
    ///   4. NpcDeathHandler.OnPlayerKilled() — 暴露掉落、尸体创建
    ///
    /// 如果 PlayerDyingEvent.CancelDeath 被设为 true，跳过后续处理。
    ///
    /// 注意：交互键检测不在 PreUpdate 中进行，因为此时 Player.controlUseTile
    /// 尚未更新（仍是上一帧的值）。交互检测统一在 LootSystem.PostUpdateWorld 中处理。
    /// </summary>
    public class CorpsePlayer : ModPlayer
    {
        private bool _wasDead;

        public override void PreUpdate()
        {
            // 检测玩家死亡状态变化（从活着→死亡）
            if (!_wasDead && Player.dead && Player.whoAmI == Main.myPlayer)
            {
                // Step 1: 发布 PlayerDyingEvent（供 ChunQiuChanPlayer 等外部系统监听）
                var evt = new PlayerDyingEvent
                {
                    Player = Player,
                    KillerNPCType = null,
                    IsRebirthProtected = false,
                    CancelDeath = false
                };
                EventBus.Publish(evt);

                // 如果事件被取消（如春秋蝉回溯），跳过后续死亡处理
                if (evt.CancelDeath)
                    return;

                // Step 2: 处理空窍蛊虫（逃跑/自毁/保留）
                var kqPlayer = Player.GetModPlayer<KongQiaoPlayer>();
                kqPlayer?.OnPlayerDeath();

                // Step 3: 清空真元
                var qiPlayer = Player.GetModPlayer<QiResourcePlayer>();
                qiPlayer?.OnDeathClearQi();

                // Step 4: 处理暴露掉落和尸体生成
                NpcDeathHandler.Instance.OnPlayerKilled(Player, null);
            }
            _wasDead = Player.dead;
        }
    }
}
