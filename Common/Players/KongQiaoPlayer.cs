using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.Events;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.Projectiles;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Common.Players
{
    /// <summary>
    /// 空窍系统（全新，与旧 QiPlayer 无冲突）
    /// 职责：蛊虫炼化/取出/启用/休眠、容量管理、死亡处理
    /// 不处理：真元存储、境界计算、资质加成
    /// </summary>
    public class KongQiaoPlayer : ModPlayer
    {
        /// <summary>空窍中的蛊虫列表</summary>
        public List<KongQiaoSlot> KongQiao = new();

        /// <summary>上次死亡时损失的蛊虫类型ID列表（供悔蛊恢复使用）</summary>
        public List<int> LostGusOnDeath = new();

        /// <summary>空窍格子数硬上限</summary>
        public int MaxSlots;

        /// <summary>已使用的格子数</summary>
        public int UsedSlots => KongQiao.Count;

        /// <summary>
        /// 设置最大格子数。由 QiRealmPlayer 在境界变化时调用。
        /// 超出部分自动休眠。
        /// </summary>
        public void SetMaxSlots(int slots)
        {
            MaxSlots = slots;
            for (int i = slots; i < KongQiao.Count; i++)
                KongQiao[i].IsActive = false;
            RecalculateQiOccupied();
        }

        /// <summary>
        /// 尝试炼化蛊虫进入空窍。
        /// </summary>
        public bool TryRefineGu(Item guItem)
        {
            if (KongQiao.Count >= MaxSlots) return false;

            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            float refineCost = guItem.damage * 2f;
            if (!qiResource.ConsumeQi(refineCost)) return false;

            var slot = new KongQiaoSlot
            {
                GuItem = guItem.Clone(),
                IsActive = false,
                QiOccupation = CalculateQiOccupation(guItem),
                GuTypeID = guItem.type,
                IsAttackGu = guItem.damage > 0,
                IsPassiveGu = guItem.defense > 0 || guItem.ModItem is Content.Items.Accessories.PassiveGuItem,
                IsMainGu = KongQiao.Count == 0,
                Loyalty = 50f,
                BuffType = 0
            };

            // 从 PassiveGuItem 读取效果数据存入槽位
            if (guItem.ModItem is Content.Items.Accessories.PassiveGuItem passive)
            {
                slot.DefenseBonus = passive.DefenseBonus;
                slot.DamageReduction = passive.DamageReduction;
                slot.BuffType = passive.BuffType;
                slot.MoveSpeedBonus = passive.MoveSpeedBonus;
                slot.LifeBonus = passive.LifeBonus;
                slot.WingCanFly = passive.WingCanFly;
                slot.WingFlightTimeMax = passive.WingFlightTimeMax;
                slot.WingAscentSpeed = passive.WingAscentSpeed;
                slot.IsPassiveGu = true;
            }

            KongQiao.Add(slot);
            guItem.TurnToAir();
            Main.NewText($"炼化成功！{slot.GuItem.Name}已进入空窍", Color.Green);
            return true;
        }

        /// <summary>
        /// 尝试从空窍取出蛊虫。
        /// 如果取出的是本命蛊，则自动将下一个蛊虫提升为本命蛊。
        /// </summary>
        public bool TryExtractGu(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= KongQiao.Count) return false;

            bool wasMainGu = KongQiao[slotIndex].IsMainGu;

            Player.QuickSpawnItem(Player.GetSource_GiftOrReward(), KongQiao[slotIndex].GuItem.Clone());
            KongQiao.RemoveAt(slotIndex);

            // 如果取出的是本命蛊且有剩余蛊虫，自动提升第一个为本命蛊
            if (wasMainGu && KongQiao.Count > 0)
            {
                KongQiao[0].IsMainGu = true;
                Main.NewText("本命蛊已取出，" + KongQiao[0].GuItem.Name + "自动晋升为本命蛊", Color.LightBlue);
            }

            RecalculateQiOccupied();
            Main.NewText("已从空窍取出蛊虫", Color.Green);
            return true;
        }

        /// <summary>
        /// 设置蛊虫启用/休眠状态。
        /// </summary>
        public void SetGuActive(int slotIndex, bool active)
        {
            if (slotIndex < 0 || slotIndex >= KongQiao.Count) return;

            var slot = KongQiao[slotIndex];
            if (active)
            {
                var qiResource = Player.GetModPlayer<QiResourcePlayer>();
                if (qiResource.QiAvailable < slot.QiOccupation)
                {
                    Main.NewText("真元不足，无法启用此蛊虫", Color.Red);
                    return;
                }
            }

            slot.IsActive = active;
            RecalculateQiOccupied();

            if (active)
            {
                EventBus.Publish(new GuActivatedEvent
                {
                    PlayerID = Player.whoAmI,
                    GuTypeID = slot.GuTypeID,
                    IsInFamilyCore = false
                });
            }
        }

        /// <summary>
        /// 获取已启用的攻击蛊列表。
        /// </summary>
        public List<KongQiaoSlot> GetActiveAttackGus() =>
            KongQiao.Where(s => s.IsActive && s.IsAttackGu).ToList();

        /// <summary>
        /// 获取已启用的被动蛊列表。
        /// </summary>
        public List<KongQiaoSlot> GetActivePassiveGus() =>
            KongQiao.Where(s => s.IsActive && s.IsPassiveGu).ToList();        /// <summary>
        /// 获取所有蛊虫的总占据额度（供UI显示）。
        /// </summary>
        public int GetTotalQiOccupation()
        {
            return KongQiao.Where(s => s.IsActive).Sum(s => s.QiOccupation);
        }

        private void RecalculateQiOccupied()
        {
            int occupied = GetTotalQiOccupation();
            Player.GetModPlayer<QiResourcePlayer>().UpdateQiOccupied(occupied);
        }

        /// <summary>
        /// 外部调用：重新计算占据额度（公开给系统使用）
        /// </summary>
        public void RefreshQiOccupied() => RecalculateQiOccupied();

        /// <summary>
        /// 计算蛊虫的占据额度。
        /// </summary>
        private int CalculateQiOccupation(Item guItem) => 10 + (int)(guItem.damage / 10f);

        /// <summary>
        /// D-20 / D-05 / D-06: 玩家死亡时处理空窍中的蛊虫。
        /// </summary>
        public void OnPlayerDeath()
        {
            foreach (var slot in KongQiao) slot.IsActive = false;
            RecalculateQiOccupied();

            var escaped = new List<KongQiaoSlot>();
            var selfDestructed = new List<KongQiaoSlot>();
            var retained = new List<KongQiaoSlot>();
            int mainGuTypeID = -1;

            for (int i = KongQiao.Count - 1; i >= 0; i--)
            {
                var slot = KongQiao[i];
                if (slot.IsMainGu) { retained.Add(slot); mainGuTypeID = slot.GuTypeID; continue; }

                if (slot.Loyalty < 40f)
                {
                    if (Main.rand.NextBool()) { escaped.Add(slot); KongQiao.RemoveAt(i); }
                    else { selfDestructed.Add(slot); KongQiao.RemoveAt(i); }
                }
                else retained.Add(slot);
            }

            LostGusOnDeath = escaped.Select(s => s.GuTypeID)
                .Concat(selfDestructed.Select(s => s.GuTypeID))
                .ToList();

            EventBus.Publish(new PlayerGusLostOnDeathEvent
            {
                PlayerID = Player.whoAmI,
                EscapedGuTypeIDs = escaped.Select(s => s.GuTypeID).ToList(),
                SelfDestructedGuTypeIDs = selfDestructed.Select(s => s.GuTypeID).ToList(),
                RetainedGuTypeIDs = retained.Select(s => s.GuTypeID).ToList(),
                MainGuTypeID = mainGuTypeID
            });

            if (Main.netMode == NetmodeID.SinglePlayer)
                Main.NewText($"[死亡] 空窍损失：叛逃 {escaped.Count} 只，自毁 {selfDestructed.Count} 只，保留 {retained.Count} 只（含本命蛊）", Color.OrangeRed);
        }

        public override void PostUpdate()
        {
            // 被动蛊维护消耗 + 效果应用
            foreach (var gu in GetActivePassiveGus())
            {
                var qiResource = Player.GetModPlayer<QiResourcePlayer>();
                if (!qiResource.ConsumeQi(0.5f))
                {
                    gu.IsActive = false;
                    RecalculateQiOccupied();
                    continue;
                }

                // 统一应用被动蛊效果
                if (gu.DefenseBonus > 0)
                    Player.statDefense += gu.DefenseBonus;
                if (gu.DamageReduction > 0)
                    Player.endurance += gu.DamageReduction;
                if (gu.BuffType > 0)
                    Player.AddBuff(gu.BuffType, 2);
                if (gu.MoveSpeedBonus > 0)
                    Player.moveSpeed += gu.MoveSpeedBonus;
                if (gu.LifeBonus > 0)
                    Player.statLifeMax2 += gu.LifeBonus;
                if (gu.WingCanFly && gu.WingFlightTimeMax > 0)
                {
                    Player.wingTimeMax = Math.Max(Player.wingTimeMax, gu.WingFlightTimeMax);
                    if (Player.wingTime < gu.WingFlightTimeMax)
                        Player.wingTime += gu.WingAscentSpeed;
                }
            }

            // 忠诚度缓慢增长
            foreach (var gu in KongQiao)
                if (gu.Loyalty < 100f)
                    gu.Loyalty = Math.Min(100f, gu.Loyalty + 0.001f);
        }

        // ===== 数据持久化 =====
        public override void SaveData(TagCompound tag)
        {
            tag["MaxSlots"] = MaxSlots;
            tag["LostGusOnDeath"] = LostGusOnDeath;
            var list = new List<TagCompound>();
            foreach (var slot in KongQiao)
            {
                list.Add(new TagCompound
                {
                    ["GuItem"] = slot.GuItem,
                    ["IsActive"] = slot.IsActive,
                    ["QiOccupation"] = slot.QiOccupation,
                    ["GuTypeID"] = slot.GuTypeID,
                    ["IsAttackGu"] = slot.IsAttackGu,
                    ["IsPassiveGu"] = slot.IsPassiveGu,
                    ["IsMainGu"] = slot.IsMainGu,
                    ["Loyalty"] = slot.Loyalty,
                    ["BuffType"] = slot.BuffType,
                    ["DefenseBonus"] = slot.DefenseBonus,
                    ["DamageReduction"] = slot.DamageReduction,
                    ["MoveSpeedBonus"] = slot.MoveSpeedBonus,
                    ["LifeBonus"] = slot.LifeBonus,
                    ["WingCanFly"] = slot.WingCanFly,
                    ["WingFlightTimeMax"] = slot.WingFlightTimeMax,
                    ["WingAscentSpeed"] = slot.WingAscentSpeed
                });
            }
            tag["KongQiao"] = list;
        }

        public override void LoadData(TagCompound tag)
        {
            MaxSlots = tag.GetInt("MaxSlots");
            LostGusOnDeath = tag.GetList<int>("LostGusOnDeath")?.ToList() ?? new List<int>();
            KongQiao.Clear();
            foreach (var st in tag.GetList<TagCompound>("KongQiao"))
            {
                KongQiao.Add(new KongQiaoSlot
                {
                    GuItem = st.Get<Item>("GuItem"),
                    IsActive = st.GetBool("IsActive"),
                    QiOccupation = st.GetInt("QiOccupation"),
                    GuTypeID = st.GetInt("GuTypeID"),
                    IsAttackGu = st.GetBool("IsAttackGu"),
                    IsPassiveGu = st.GetBool("IsPassiveGu"),
                    IsMainGu = st.GetBool("IsMainGu"),
                    Loyalty = st.GetFloat("Loyalty"),
                    BuffType = st.GetInt("BuffType"),
                    DefenseBonus = st.GetInt("DefenseBonus"),
                    DamageReduction = st.GetFloat("DamageReduction"),
                    MoveSpeedBonus = st.GetFloat("MoveSpeedBonus"),
                    LifeBonus = st.GetInt("LifeBonus"),
                    WingCanFly = st.GetBool("WingCanFly"),
                    WingFlightTimeMax = st.GetInt("WingFlightTimeMax"),
                    WingAscentSpeed = st.GetFloat("WingAscentSpeed")
                });
            }
            RecalculateQiOccupied();
        }
    }

    /// <summary>
    /// 空窍中的单个蛊虫槽位。
    /// </summary>
    public class KongQiaoSlot
    {
        /// <summary>蛊虫物品实例</summary>
        public Item GuItem;

        /// <summary>是否启用</summary>
        public bool IsActive;

        /// <summary>占据的真元额度</summary>
        public int QiOccupation;

        /// <summary>蛊虫类型ID</summary>
        public int GuTypeID;

        /// <summary>是否为攻击蛊</summary>
        public bool IsAttackGu;

        /// <summary>是否为被动蛊</summary>
        public bool IsPassiveGu;

        /// <summary>是否为本命蛊</summary>
        public bool IsMainGu;

        /// <summary>忠诚度 [0, 100]</summary>
        public float Loyalty;

        /// <summary>Buff类型（被动蛊用）</summary>
        public int BuffType;

        // ===== 被动蛊效果字段（炼入空窍时从 PassiveGuItem 读取） =====

        /// <summary>防御力加成</summary>
        public int DefenseBonus;

        /// <summary>伤害减免 [0, 1]</summary>
        public float DamageReduction;

        /// <summary>移速加成</summary>
        public float MoveSpeedBonus;

        /// <summary>生命上限加成</summary>
        public int LifeBonus;

        /// <summary>是否可飞行</summary>
        public bool WingCanFly;

        /// <summary>飞行时间上限（帧）</summary>
        public int WingFlightTimeMax;

        /// <summary>飞行上升速度</summary>
        public float WingAscentSpeed;
    }
}
