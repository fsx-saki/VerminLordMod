using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items;

namespace VerminLordMod.Common.Systems
{
    /// <summary>
    /// 仙蛊屋运转系统 — 每帧驱动所有玩家的激活仙蛊屋。
    /// 负责：
    /// 1. 每帧调用 GuHouseItem.OnUpdate（子类持续效果）
    /// 2. 每秒扣除 SustainQiCostPerSecond 真元
    /// 3. 真元不足时自动停用并提示
    /// 
    /// 设计参考：FormationSystem.PreUpdateWorld 的持续维护消耗模式。
    /// </summary>
    public class GuHouseSystem : ModSystem
    {
        public override void PreUpdateWorld()
        {
            foreach (var player in Main.ActivePlayers)
            {
                var housePlayer = player.GetModPlayer<GuHousePlayer>();
                if (!housePlayer.IsActive) continue;

                if (housePlayer.ActiveHouse.ModItem is not GuHouseItem gh)
                {
                    // 异常：激活槽中不是仙蛊屋，强制清理
                    housePlayer.Deactivate();
                    continue;
                }

                // 每帧调用子类持续效果
                gh.OnUpdate(player);

                // 每秒扣一次 SustainQiCostPerSecond
                housePlayer.SustainTimer++;
                if (housePlayer.SustainTimer >= 60)
                {
                    housePlayer.SustainTimer = 0;

                    var qiPlayer = player.GetModPlayer<QiResourcePlayer>();
                    if (qiPlayer.QiCurrent < gh.SustainQiCostPerSecond)
                    {
                        Main.NewText($"[{gh.Item.Name}] 真元耗尽，仙蛊屋停止运转！", Color.Red);
                        housePlayer.Deactivate();
                        continue;
                    }

                    qiPlayer.ConsumeQi(gh.SustainQiCostPerSecond);
                }

                // 限时仙蛊屋：检查持续时间
                if (gh.ActiveDuration > 0)
                {
                    // TODO: P2 实现限时逻辑（需记录激活帧数）
                }
            }
        }
    }
}
