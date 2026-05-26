using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Items.Weapons;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 涸泽蛊
    /// 四转（到北原降为三转）水道蛊
    /// 倍增下一次蛊虫攻击威力，但该蛊虫炼化度下降30点
    /// 多只涸泽蛊效果可叠加
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "完整实现：消耗品，施加涸泽增幅Buff，Buff结束时扣除炼化度", "四转", "水")]
    public class HeZeGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 30;
            Item.value = 20000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            Item heldItem = player.HeldItem;
            if (heldItem == null || heldItem.IsAir)
            {
                Text.ShowTextRed(player, "请手持蛊虫武器后再使用涸泽蛊");
                return false;
            }

            if (!(heldItem.ModItem is GuWeaponItem))
            {
                Text.ShowTextRed(player, "涸泽蛊只能增幅蛊虫武器");
                return false;
            }

            int buffType = ModContent.BuffType<HeZeBuff>();
            if (player.HasBuff(buffType))
            {
                int buffIndex = player.FindBuffIndex(buffType);
                if (buffIndex >= 0)
                {
                    player.buffTime[buffIndex] += 300;
                    HeZeBuff.AddStack(player.whoAmI);
                    Text.ShowTextGreen(player, $"涸泽增幅叠加！当前层数：{HeZeBuff.GetStacks(player.whoAmI)}");
                }
            }
            else
            {
                player.AddBuff(buffType, 300);
                HeZeBuff.SetStacks(player.whoAmI, 1);
                Text.ShowTextGreen(player, "涸泽增幅已激活！蛊虫攻击威力倍增");
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HeZeDesc", "倍增下一次蛊虫攻击威力，但该蛊虫炼化度下降30点"));
            tooltips.Add(new TooltipLine(Mod, "HeZeStack", "多只涸泽蛊效果可叠加"));
            tooltips.Add(new TooltipLine(Mod, "HeZeCost", "使用时需手持蛊虫武器"));
        }
    }
}
