using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 玩家尸体处理器
    ///
    /// 钩住玩家死亡事件，触发 NpcDeathHandler.OnPlayerKilled 生成尸体。
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
                // 触发死亡处理
                NpcDeathHandler.Instance.OnPlayerKilled(Player, null);
            }
            _wasDead = Player.dead;
        }
    }
}
