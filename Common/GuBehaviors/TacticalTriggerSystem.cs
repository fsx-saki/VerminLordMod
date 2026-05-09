using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.GuBehaviors
{
    /// <summary>
    /// 战术触发系统（D-29）
    ///
    /// 将 ITacticalTriggerProvider 集成到战斗循环中。
    /// 在弹幕命中时检查来源武器是否实现了 ITacticalTriggerProvider，
    /// 评估触发条件并应用对应的战术Buff。
    /// </summary>
    public static class TacticalTriggerSystem
    {
        /// <summary>
        /// 在弹幕命中NPC时检查并应用战术触发。
        /// 由 GuProjectileInfo.OnHitNPC 调用。
        /// </summary>
        public static void OnProjectileHit(Projectile projectile, NPC target, Player player)
        {
            if (player == null || !player.active) return;
            if (target == null || !target.active) return;

            var triggerPlayer = player.GetModPlayer<TacticalTriggerPlayer>();
            if (triggerPlayer == null) return;

            // 1. 注册命中（连击计数）
            triggerPlayer.RegisterHit();

            // 2. 检查背击
            bool isBackstab = triggerPlayer.CheckBackstab(target, projectile);

            // 3. 获取来源物品的 ITacticalTriggerProvider
            Item sourceItem = player.HeldItem;
            if (sourceItem == null || sourceItem.IsAir) return;

            if (sourceItem.ModItem is ITacticalTriggerProvider provider)
            {
                // 4. 调用 CheckTriggers 获取武器定义的触发Buff
                provider.CheckTriggers(player, out var weaponBuffs);

                // 5. 合并事件触发的Buff（连击、背击、击杀、受击、队友死亡）
                var eventBuffs = EvaluateEventTriggers(triggerPlayer, target, isBackstab);

                // 6. 合并所有Buff
                triggerPlayer.ActiveBuffs.Clear();
                triggerPlayer.ActiveBuffs.AddRange(weaponBuffs);
                triggerPlayer.ActiveBuffs.AddRange(eventBuffs);
            }
        }

        /// <summary>
        /// 在击杀NPC时检查触发。
        /// 由 NpcDeathHandler 或其他击杀事件调用。
        /// </summary>
        public static void OnNPCKilled(Player player, NPC npc)
        {
            if (player == null || !player.active) return;
            var triggerPlayer = player.GetModPlayer<TacticalTriggerPlayer>();
            if (triggerPlayer == null) return;

            // 击杀触发 — 添加 OnKill 标记Buff
            triggerPlayer.ActiveBuffs.Add(new ActiveTacticalBuff
            {
                Trigger = TacticalTrigger.OnKill,
                RemainingFrames = 120, // 2秒
                DamageMultiplier = 1.25f,
                LifeStealBonus = 0.05f,
            });
        }

        /// <summary>
        /// 在玩家受击时检查触发。
        /// 由 EffectsPlayer.OnHitByNPC 或其他受击事件调用。
        /// </summary>
        public static void OnPlayerHitTaken(Player player)
        {
            if (player == null || !player.active) return;
            var triggerPlayer = player.GetModPlayer<TacticalTriggerPlayer>();
            if (triggerPlayer == null) return;

            triggerPlayer.RegisterHitTaken();

            // 受击5次触发
            if (triggerPlayer.HitTakenCount >= 5)
            {
                triggerPlayer.ActiveBuffs.Add(new ActiveTacticalBuff
                {
                    Trigger = TacticalTrigger.OnHitTaken5,
                    RemainingFrames = 180, // 3秒
                    DefenseBonus = 0.2f,
                    DamageMultiplier = 1.1f,
                });
                triggerPlayer.HitTakenCount = 0; // 重置计数
            }
        }

        /// <summary>
        /// 评估事件触发的条件（连击、背击、击杀、受击、队友死亡）。
        /// </summary>
        private static List<ActiveTacticalBuff> EvaluateEventTriggers(
            TacticalTriggerPlayer triggerPlayer, NPC target, bool isBackstab)
        {
            var buffs = new List<ActiveTacticalBuff>();

            // 连击10次触发
            if (triggerPlayer.ComboCount >= 10)
            {
                buffs.Add(new ActiveTacticalBuff
                {
                    Trigger = TacticalTrigger.OnCombo10,
                    RemainingFrames = 60, // 1秒
                    DamageMultiplier = 1.3f,
                    CritBonus = 0.15f,
                });
                triggerPlayer.ComboCount = 0; // 重置连击
            }

            // 背击触发
            if (isBackstab)
            {
                buffs.Add(new ActiveTacticalBuff
                {
                    Trigger = TacticalTrigger.OnBackstab,
                    RemainingFrames = 30, // 0.5秒
                    DamageMultiplier = 1.5f,
                    CritBonus = 0.25f,
                });
            }

            // 完美闪避触发
            if (triggerPlayer.CheckPerfectDodge())
            {
                buffs.Add(new ActiveTacticalBuff
                {
                    Trigger = TacticalTrigger.OnPerfectDodge,
                    RemainingFrames = 60, // 1秒
                    DamageMultiplier = 1.4f,
                    SpeedMultiplier = 1.3f,
                });
                triggerPlayer.PerfectDodgeWindowActive = false; // 消耗闪避窗口
            }

            // 队友死亡触发
            if (triggerPlayer.AllyDeathFlag)
            {
                buffs.Add(new ActiveTacticalBuff
                {
                    Trigger = TacticalTrigger.OnAllyDeath,
                    RemainingFrames = 300, // 5秒
                    DamageMultiplier = 1.35f,
                    DefenseBonus = 0.15f,
                });
                triggerPlayer.AllyDeathFlag = false; // 消耗标记
            }

            return buffs;
        }
    }
}
