using Terraria;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Items.Weapons.Six;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 玩家状态快照（D-14）。
    ///
    /// 为 NPC 感知系统提供统一的玩家状态快照，避免每次感知时重复计算。
    /// 由 PerceptionContext 引用，在 PostUpdate 或感知触发时刷新。
    ///
    /// 字段说明：
    /// - LifePercent: 当前生命值百分比 [0, 100]
    /// - QiLevel: 当前灵气值百分比 [0, 100]（来自 QiResourcePlayer）
    /// - RealmLevel: 当前修炼境界等级（来自 QiRealmPlayer）
    /// - ActiveGuCount: 当前激活的蛊虫数量（来自 KongQiaoPlayer）
    /// - DaoHenMultiplier: 当前道痕总倍率（来自 DaoHenPlayer）
    /// - HasChunQiuChan: 是否持有春秋蝉（通过检查背包中的 ChunQiuChan 物品）
    /// - InfamyLevel: 恶名等级（来自 GuWorldPlayer）
    /// - AlliedFactions: 当前结盟的家族列表（来自 GuWorldPlayer）
    /// </summary>
    public struct PlayerStateSnapshot
    {
        /// <summary> 当前生命值百分比 [0, 100] </summary>
        public int LifePercent;

        /// <summary> 当前灵气值百分比 [0, 100] </summary>
        public int QiLevel;

        /// <summary> 当前修炼境界等级 </summary>
        public int RealmLevel;

        /// <summary> 当前激活的蛊虫数量 </summary>
        public int ActiveGuCount;

        /// <summary> 当前道痕总倍率 </summary>
        public float DaoHenMultiplier;

        /// <summary> 是否持有春秋蝉 </summary>
        public bool HasChunQiuChan;

        /// <summary> 恶名等级（0=无名, 1=小有名气, 2=臭名昭著, 3=恶贯满盈） </summary>
        public int InfamyLevel;

        /// <summary> 当前结盟的家族列表 </summary>
        public FactionID[] AlliedFactions;

        /// <summary>
        /// 从指定玩家刷新快照数据。
        /// </summary>
        public static PlayerStateSnapshot TakeSnapshot(Player player)
        {
            var snapshot = new PlayerStateSnapshot();

            // 生命值
            snapshot.LifePercent = player.statLifeMax2 > 0
                ? (player.statLife * 100) / player.statLifeMax2
                : 0;

            // 灵气值
            var qiPlayer = player.GetModPlayer<QiResourcePlayer>();
            if (qiPlayer != null && qiPlayer.QiMaxCurrent > 0)
            {
                snapshot.QiLevel = (int)((qiPlayer.QiCurrent * 100f) / qiPlayer.QiMaxCurrent);
            }

            // 修炼境界
            var realmPlayer = player.GetModPlayer<QiRealmPlayer>();
            if (realmPlayer != null)
            {
                snapshot.RealmLevel = realmPlayer.GuLevel;
            }

            // 激活的蛊虫数量
            var kqPlayer = player.GetModPlayer<KongQiaoPlayer>();
            if (kqPlayer != null)
            {
                int count = 0;
                foreach (var slot in kqPlayer.KongQiao)
                {
                    if (slot != null && slot.IsActive)
                        count++;
                }
                snapshot.ActiveGuCount = count;
            }

            // 道痕倍率（使用第一个 DaoType 值作为参考获取总倍率）
            var daoHenPlayer = player.GetModPlayer<DaoHenPlayer>();
            if (daoHenPlayer != null)
            {
                snapshot.DaoHenMultiplier = daoHenPlayer.GetMultiplier(GuBehaviors.DaoType.Ban);
            }

            // 春秋蝉：检查背包中是否有 ChunQiuChan 物品
            snapshot.HasChunQiuChan = false;
            for (int i = 0; i < player.inventory.Length; i++)
            {
                var item = player.inventory[i];
                if (item != null && !item.IsAir && item.ModItem is ChunQiuChan)
                {
                    snapshot.HasChunQiuChan = true;
                    break;
                }
            }

            // 恶名
            var gwPlayer = player.GetModPlayer<GuWorldPlayer>();
            if (gwPlayer != null)
            {
                int infamy = gwPlayer.InfamyPoints;
                if (infamy >= 500) snapshot.InfamyLevel = 3;
                else if (infamy >= 200) snapshot.InfamyLevel = 2;
                else if (infamy >= 50) snapshot.InfamyLevel = 1;
                else snapshot.InfamyLevel = 0;

                // 结盟家族
                var allies = new System.Collections.Generic.List<FactionID>();
                foreach (var kvp in gwPlayer.FactionRelations)
                {
                    if (kvp.Value.IsAllied)
                        allies.Add(kvp.Key);
                }
                snapshot.AlliedFactions = allies.ToArray();
            }
            else
            {
                snapshot.AlliedFactions = System.Array.Empty<FactionID>();
            }

            return snapshot;
        }
    }
}
