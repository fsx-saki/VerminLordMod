using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "八转炼道仙蛊-悔池", "八转", "炼")]
    public class HuiGu : ModItem
    {
        private const int QiCostPerUse = 100;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 3;
            Item.value = 1000000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动悔蛊");
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            var kongQiao = player.GetModPlayer<KongQiaoPlayer>();

            if (kongQiao.LostGusOnDeath != null && kongQiao.LostGusOnDeath.Count > 0)
            {
                qiResource.ConsumeQi(QiCostPerUse);

                int index = Main.rand.Next(kongQiao.LostGusOnDeath.Count);
                int guTypeID = kongQiao.LostGusOnDeath[index];
                kongQiao.LostGusOnDeath.RemoveAt(index);

                player.QuickSpawnItem(player.GetSource_GiftOrReward(), guTypeID, 1);

                Text.ShowTextGreen(player, "悔池生效！已恢复一只损失的蛊虫");
                return true;
            }

            float refundAmount = QiCostPerUse * 0.5f;
            qiResource.ConsumeQi(QiCostPerUse - refundAmount);
            qiResource.RefundQi(refundAmount);

            Text.ShowTextRed(player, "无损失蛊虫可恢复，已返还50%真元");
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HuiGuDesc", "八转炼道仙蛊：悔池"));
            tooltips.Add(new TooltipLine(Mod, "HuiGuEffect", "恢复上次死亡时损失的一只蛊虫"));
            tooltips.Add(new TooltipLine(Mod, "HuiGuNoLoss", "若无损失蛊虫，返还50%真元消耗"));
            tooltips.Add(new TooltipLine(Mod, "HuiGuQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "HuiGuConsumable", "[c/FF6666:消耗品 - 使用后消失]"));
        }
    }
}
