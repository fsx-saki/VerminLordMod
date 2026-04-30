using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 真元资源系统（从 QiPlayer 拆分）
    /// 职责：真元存储、消耗、恢复、死亡清空
    /// 不处理：境界计算、资质加成、战斗数值修改
    /// </summary>
    public class QiResourcePlayer : ModPlayer
    {
        // ===== 核心字段 =====
        /// <summary>基础真元上限（由 QiRealmPlayer 写入）</summary>
        public float QiMaxBase;

        /// <summary>当前真元上限（每帧 ResetEffects 中 = QiMaxBase，再叠加装备/Buff）</summary>
        public float QiMaxCurrent;

        /// <summary>当前真元</summary>
        public float QiCurrent;

        /// <summary>被启用蛊虫占据的额度（由 KongQiaoPlayer 调用 UpdateQiOccupied 写入）</summary>
        public int QiOccupied;

        /// <summary>可用真元 = 当前上限 - 占据额度</summary>
        public int QiAvailable => (int)QiMaxCurrent - QiOccupied;

        /// <summary>基础恢复速率（由 QiRealmPlayer 写入）</summary>
        public float BaseQiRegenRate;

        /// <summary>额外恢复加成（由装备/Buff/酒虫通过 ExtraQiRegen 写入）</summary>
        public float ExtraQiRegen;

        /// <summary>离散恢复计时器</summary>
        private int regenTimer;

        // ===== 方法 =====

        /// <summary>
        /// 被动接收占据额度更新。由 KongQiaoPlayer 在蛊虫启用/休眠时调用。
        /// 禁止其他系统直接修改 QiOccupied 字段。
        /// </summary>
        public void UpdateQiOccupied(int occupied) => QiOccupied = occupied;

        /// <summary>
        /// 消耗真元。返回是否成功。
        /// </summary>
        public bool ConsumeQi(float amount)
        {
            if (QiCurrent < amount) return false;
            float oldQi = QiCurrent;
            QiCurrent -= amount;
            EventBus.Publish(new PlayerQiChangedEvent
            {
                PlayerID = Player.whoAmI,
                OldQi = oldQi,
                NewQi = QiCurrent,
                Reason = QiChangeReason.Consume
            });
            return true;
        }

        /// <summary>
        /// 返还真元（六转以上使用低阶蛊虫等场景）。
        /// </summary>
        public void RefundQi(float amount)
        {
            QiCurrent = MathHelper.Min(QiMaxCurrent, QiCurrent + amount);
        }

        /// <summary>
        /// D-20: 死亡完全清空真元（覆盖旧版的减半行为）。
        /// </summary>
        public void OnDeathClearQi()
        {
            float oldQi = QiCurrent;
            QiCurrent = 0;
            EventBus.Publish(new PlayerQiChangedEvent
            {
                PlayerID = Player.whoAmI,
                OldQi = oldQi,
                NewQi = 0,
                Reason = QiChangeReason.DeathClear
            });
        }

        /// <summary>
        /// 每帧离散恢复。
        /// 完全保留旧代码的离散机制：计时器 + 每跳恢复整点。
        /// </summary>
        public override void PostUpdate()
        {
            float effectiveRegen = BaseQiRegenRate + ExtraQiRegen;
            if (effectiveRegen <= 0 || QiCurrent >= QiMaxCurrent) return;

            regenTimer++;
            int regenPerTick = 1;
            int interval = (int)(60f / effectiveRegen);

            if (effectiveRegen > 60)
            {
                regenPerTick = (int)(effectiveRegen / 60f);
                interval = 1;
            }

            if (regenTimer >= interval)
            {
                regenTimer = 0;
                QiCurrent = MathHelper.Min(QiMaxCurrent, QiCurrent + regenPerTick);
            }
        }

        /// <summary>
        /// 每帧重置：当前上限回到基础值，额外恢复归零。
        /// 装备/Buff 在 PostUpdateEquips 中叠加到 ExtraQiRegen。
        /// </summary>
        public override void ResetEffects()
        {
            QiMaxCurrent = QiMaxBase;
            ExtraQiRegen = 0;
        }

        /// <summary>
        /// 重置所有资源数据（用于调试/开新档）。
        /// </summary>
        public void ResetAll()
        {
            QiMaxBase = 0;
            QiMaxCurrent = 0;
            QiCurrent = 0;
            QiOccupied = 0;
            BaseQiRegenRate = 0;
            ExtraQiRegen = 0;
            regenTimer = 0;
        }

        /// <summary>
        /// 死亡时清空真元。
        /// </summary>
        public override void UpdateDead()
        {
            OnDeathClearQi();
        }

        // ===== 数据持久化 =====
        public override void SaveData(TagCompound tag)
        {
            tag["QiCurrent"] = QiCurrent;
            tag["QiMaxBase"] = QiMaxBase;
            tag["QiMaxCurrent"] = QiMaxCurrent;
            tag["BaseQiRegenRate"] = BaseQiRegenRate;
            tag["QiOccupied"] = QiOccupied;
        }

        public override void LoadData(TagCompound tag)
        {
            QiCurrent = tag.GetFloat("QiCurrent");
            QiMaxBase = tag.GetFloat("QiMaxBase");
            QiMaxCurrent = tag.GetFloat("QiMaxCurrent");
            BaseQiRegenRate = tag.GetFloat("BaseQiRegenRate");
            QiOccupied = tag.GetInt("QiOccupied");
        }
    }
}
