using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Abstractions;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items
{
    /// <summary>
    /// 仙蛊屋物品基类
    /// 仙蛊屋是由多只仙蛊组合而成的建筑型法宝，具有独立的等级体系和特殊能力。
    /// 继承此类可实现具体的仙蛊屋物品。
    /// </summary>
    public abstract class GuHouseItem : ModItem, IGuHouse
    {
        // ============================================================
        // IGuHouse 接口实现 — 子类可重写
        // ============================================================

        /// <summary>仙蛊屋等级（七转起步，最高九转）</summary>
        public abstract int HouseLevel { get; }

        /// <summary>仙蛊屋类型</summary>
        public abstract GuHouseType HouseType { get; }

        /// <summary>组成仙蛊屋的核心仙蛊列表（类型ID）</summary>
        public virtual List<int> ComponentGuTypes => new();

        /// <summary>仙蛊屋激活所需真元</summary>
        public abstract int ActivationQiCost { get; }

        /// <summary>仙蛊屋持续运行每秒消耗真元</summary>
        public abstract int SustainQiCostPerSecond { get; }

        /// <summary>仙蛊屋覆盖范围（像素）</summary>
        public virtual float Range => 400f;

        /// <summary>仙蛊屋是否可移动</summary>
        public virtual bool IsMobile => false;

        // ============================================================
        // 基础属性
        // ============================================================

        /// <summary>仙蛊屋稀有度颜色</summary>
        protected virtual int HouseRarity => HouseLevel switch
        {
            7 => ItemRarityID.Lime,
            8 => ItemRarityID.Cyan,
            9 => ItemRarityID.Red,
            _ => ItemRarityID.LightRed,
        };

        /// <summary>仙蛊屋价值</summary>
        protected virtual int HouseValue => HouseLevel switch
        {
            7 => 500000,
            8 => 1000000,
            9 => 5000000,
            _ => 100000,
        };

        /// <summary>仙蛊屋激活后持续时长（帧，-1为无限）</summary>
        public virtual int ActiveDuration => -1;

        // ============================================================
        // SetDefaults
        // ============================================================

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.rare = HouseRarity;
            Item.maxStack = 1;
            Item.value = HouseValue;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item1;
            Item.noMelee = true;
            Item.consumable = false;
        }

        // ============================================================
        // 使用效果 — 激活仙蛊屋
        // ============================================================

        public override bool? UseItem(Player player)
        {
            var qiPlayer = player.GetModPlayer<QiResourcePlayer>();

            // 检查真元是否足够激活
            if (qiPlayer.QiCurrent < ActivationQiCost)
            {
                Main.NewText($"[{Item.Name}] 真元不足，无法激活仙蛊屋！需要 {ActivationQiCost} 真元。", Color.Red);
                return false;
            }

            // 消耗真元激活
            qiPlayer.ConsumeQi(ActivationQiCost);

            // 触发激活效果
            OnActivate(player);

            return true;
        }

        // ============================================================
        // 可重写的激活逻辑
        // ============================================================

        /// <summary>
        /// 仙蛊屋激活时触发 — 子类重写以实现具体效果
        /// </summary>
        public virtual void OnActivate(Player player)
        {
            Main.NewText($"[{Item.Name}] 仙蛊屋已激活！等级：{HouseLevel}转{HouseType}型", Color.Gold);
        }

        /// <summary>
        /// 仙蛊屋每帧更新逻辑 — 子类重写
        /// </summary>
        public virtual void OnUpdate(Player player)
        {
            // 持续消耗真元
            var qiPlayer = player.GetModPlayer<QiResourcePlayer>();
            if (SustainQiCostPerSecond > 0 && player.itemAnimation == 0)
            {
                qiPlayer.ConsumeQi(SustainQiCostPerSecond / 60);
            }
        }

        /// <summary>
        /// 仙蛊屋停用时触发
        /// </summary>
        public virtual void OnDeactivate(Player player)
        {
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            // 添加仙蛊屋信息到工具提示
            var houseLevelLine = new TooltipLine(Mod, "GuHouseLevel", $"仙蛊屋等级：{HouseLevel}转")
            {
                OverrideColor = HouseRarity switch
                {
                    7 => new Microsoft.Xna.Framework.Color(0, 255, 0),
                    8 => new Microsoft.Xna.Framework.Color(0, 255, 255),
                    9 => new Microsoft.Xna.Framework.Color(255, 0, 0),
                    _ => new Microsoft.Xna.Framework.Color(255, 255, 0),
                }
            };
            tooltips.Add(houseLevelLine);

            tooltips.Add(new TooltipLine(Mod, "GuHouseType", $"类型：{HouseType}型"));
            tooltips.Add(new TooltipLine(Mod, "GuHouseQiCost", $"激活消耗：{ActivationQiCost} 真元"));
            tooltips.Add(new TooltipLine(Mod, "GuHouseRange", $"覆盖范围：{Range} 像素"));

            if (IsMobile)
                tooltips.Add(new TooltipLine(Mod, "GuHouseMobile", "可移动仙蛊屋"));

            if (ComponentGuTypes.Count > 0)
                tooltips.Add(new TooltipLine(Mod, "GuHouseComponents", $"核心仙蛊：{ComponentGuTypes.Count} 只"));
        }
    }
}
