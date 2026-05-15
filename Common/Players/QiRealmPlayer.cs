using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Content.Items.Consumables;
using VerminLordMod.Content.Items.Weapons;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 境界系统（从 QiPlayer 拆分）
    /// 职责：境界存储、突破、破境提示、真元上限计算、空窍格子数计算
    /// 不处理：真元存储、资质加成、战斗数值修改
    /// </summary>
    public class QiRealmPlayer : ModPlayer
    {
        // ===== 核心字段 =====
        /// <summary>转数 1-10</summary>
        public int GuLevel;

        /// <summary>0=初期, 1=中期, 2=后期, 3=巅峰</summary>
        public int LevelStage;

        /// <summary>突破进度 [0, 100]</summary>
        public float BreakthroughProgress;

        // 旧代码常量保留
        private const int UNIT_KONGQIAO = 100;
        private const int BASE_KONGQIAO_SLOTS = 3;

        /// <summary>
        /// 开窍初始化。由 AwakeningSystem 或资质道具调用。
        /// </summary>
        public void OnAwakening()
        {
            GuLevel = 1;
            LevelStage = 0;
            ApplyRealmEffects(initialFill: true);

            EventBus.Publish(new PlayerRealmUpEvent
            {
                PlayerID = Player.whoAmI,
                NewLevel = GuLevel,
                NewStage = LevelStage
            });

            // 开窍时给予蛊道媒介
            GiveMediumWeapon();
        }

        /// <summary>
        /// 给予蛊道媒介（检查唯一性）
        /// </summary>
        private void GiveMediumWeapon()
        {
            // 检查背包是否已有媒介
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                if (Player.inventory[i].type == ModContent.ItemType<GuMediumWeapon>())
                    return; // 已有媒介
            }

            // 给予媒介
            var medium = new Item();
            medium.SetDefaults(ModContent.ItemType<GuMediumWeapon>());
            Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), medium);
            Main.NewText("蛊道媒介已发放，请使用空窍中的蛊虫进行攻击", Microsoft.Xna.Framework.Color.LightBlue);
        }

        /// <summary>
        /// 获取突破所需的元石数量（D-33：元石经济闭环）。
        /// </summary>
        private int GetBreakthroughCost()
        {
            // 小境界突破：10/20/30
            // 大境界突破：50 + 转数 * 10
            if (LevelStage < 3)
            {
                return (LevelStage + 1) * 10;
            }
            else
            {
                return 50 + GuLevel * 10;
            }
        }

        /// <summary>
        /// 消耗玩家背包中的元石。
        /// </summary>
        private bool ConsumeYuanStones(int amount)
        {
            if (amount <= 0) return true;

            int totalStones = 0;
            int yuanSItemType = ModContent.ItemType<YuanS>();

            // 先统计背包中的元石总数
            for (int i = 0; i < Player.inventory.Length; i++)
            {
                if (Player.inventory[i].type == yuanSItemType)
                {
                    totalStones += Player.inventory[i].stack;
                }
            }

            if (totalStones < amount)
            {
                Main.NewText($"元石不足！需要 {amount} 块元石，当前仅有 {totalStones} 块。", Color.Red);
                return false;
            }

            // 消耗元石
            int remaining = amount;
            for (int i = 0; i < Player.inventory.Length && remaining > 0; i++)
            {
                if (Player.inventory[i].type == yuanSItemType)
                {
                    int toRemove = System.Math.Min(remaining, Player.inventory[i].stack);
                    Player.inventory[i].stack -= toRemove;
                    remaining -= toRemove;
                    if (Player.inventory[i].stack <= 0)
                    {
                        Player.inventory[i].TurnToAir();
                    }
                }
            }

            Main.NewText($"消耗了 {amount} 块元石用于突破。", Color.Cyan);
            return true;
        }

        /// <summary>
        /// 小阶段突破。保留旧代码的破境提示和真元回满。
        /// D-33：突破时消耗元石。
        /// </summary>
        public void StageUp()
        {
            if (LevelStage >= 3) return;

            // D-33：检查并消耗元石
            int cost = GetBreakthroughCost();
            if (!ConsumeYuanStones(cost))
            {
                Main.NewText($"元石不足，无法突破！需要 {cost} 块元石。", Color.Red);
                return;
            }

            LevelStage++;

            string stageName = LevelStage switch
            {
                0 => "初期",
                1 => "中期",
                2 => "后期",
                3 => "巅峰",
                _ => ""
            };
            Main.NewText($"破境成功！当前境界为{GuLevel}转{stageName}", Color.Green);

            ApplyRealmEffects(initialFill: false);
            // 破境后真元回满
            Player.GetModPlayer<QiResourcePlayer>().QiCurrent =
                Player.GetModPlayer<QiResourcePlayer>().QiMaxCurrent;

            EventBus.Publish(new PlayerRealmUpEvent
            {
                PlayerID = Player.whoAmI,
                NewLevel = GuLevel,
                NewStage = LevelStage
            });
        }

        /// <summary>
        /// 大境界突破。
        /// D-33：突破时消耗元石。
        /// </summary>
        public void LevelUp()
        {
            // D-33：检查并消耗元石
            int cost = GetBreakthroughCost();
            if (!ConsumeYuanStones(cost))
            {
                Main.NewText($"元石不足，无法突破！需要 {cost} 块元石。", Color.Red);
                return;
            }

            GuLevel++;
            LevelStage = 0;
            ApplyRealmEffects(initialFill: false);
            Player.GetModPlayer<QiResourcePlayer>().QiCurrent =
                Player.GetModPlayer<QiResourcePlayer>().QiMaxCurrent;

            EventBus.Publish(new PlayerRealmUpEvent
            {
                PlayerID = Player.whoAmI,
                NewLevel = GuLevel,
                NewStage = LevelStage
            });
        }

        /// <summary>
        /// 统一应用境界效果。
        /// 保留旧 SetQis 公式用于真元上限和恢复速率（保证数值感受不变）。
        /// 引入新格子数公式（D-04 双轨制中的硬上限）。
        /// </summary>
        public void ApplyRealmEffects(bool initialFill)
        {
            var talent = Player.GetModPlayer<QiTalentPlayer>();
            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            var kongQiao = Player.GetModPlayer<KongQiaoPlayer>();

            // 1. 计算真元上限（复用旧公式：times * UNIT_KONGQIAO * 资质 / 10）
            int times = (int)Math.Pow(10, GuLevel - 1) * (int)Math.Pow(2, LevelStage);
            float maxQi = times * UNIT_KONGQIAO * talent.GetZiZhiMultiplier() / 10f;

            // 2. 计算恢复速率（复用旧公式：qiLevel * 资质 / 2）
            float regenRate = GuLevel * talent.GetZiZhiMultiplier() / 2f;

            // 3. 计算空窍格子数（新公式：D-04 硬上限）
            int slots = BASE_KONGQIAO_SLOTS + (GuLevel - 1) * 2 + LevelStage;

            // 4. 写入其他 ModPlayer
            qiResource.QiMaxBase = maxQi;
            qiResource.BaseQiRegenRate = regenRate;
            kongQiao.SetMaxSlots(slots);

            if (initialFill)
                qiResource.QiCurrent = maxQi;
        }

        /// <summary>
        /// 重置所有境界数据（用于开新档/调试）。
        /// </summary>
        public void ResetAll()
        {
            GuLevel = 0;
            LevelStage = 0;
            BreakthroughProgress = 0;
        }

        /// <summary>
        /// 每帧自动积累突破进度，满100时触发破境。
        /// 从 QiPlayer.UpdateResource 迁移。
        /// </summary>
        public override void PostUpdateMiscEffects()
        {
            if (GuLevel <= 0) return;

            if (BreakthroughProgress >= 100)
            {
                BreakthroughProgress = 0;
                StageUp();
            }
            else
            {
                BreakthroughProgress -= 0.01f;
            }

            BreakthroughProgress = Utils.Clamp(BreakthroughProgress, 0, 100);
        }

        // ===== 数据持久化 =====
        public override void SaveData(TagCompound tag)
        {
            tag["GuLevel"] = GuLevel;
            tag["LevelStage"] = LevelStage;
            tag["BreakthroughProgress"] = BreakthroughProgress;
        }

        public override void LoadData(TagCompound tag)
        {
            GuLevel = tag.GetInt("GuLevel");
            LevelStage = tag.GetInt("LevelStage");
            BreakthroughProgress = tag.GetFloat("BreakthroughProgress");
        }
    }
}
