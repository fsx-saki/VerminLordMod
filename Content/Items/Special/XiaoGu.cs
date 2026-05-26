using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转炼道辅助蛊", "一转", "炼")]
    public class XiaoGu : ModItem
    {
        private const int QiCostPerUse = 5;
        private const int BuffDuration = 480;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 500000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int buffType = ModContent.BuffType<XiaoGuBuff>();
            player.AddBuff(buffType, BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "XiaoGuEffect", "小蛊：+3%伤害，+3%防御"));
            tooltips.Add(new TooltipLine(Mod, "XiaoGuDuration", $"持续：{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "XiaoGuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
