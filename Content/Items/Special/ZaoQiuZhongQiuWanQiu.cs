using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转宙道功能蛊屋", "三转", "宙")]
    public class ZaoQiuZhongQiuWanQiu : ModItem
    {
        private const int QiCostPerUse = 20;
        private const int BuffDuration = 600;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 50000;
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

            int buffType = ModContent.BuffType<ZaoQiuBuff>();
            player.AddBuff(buffType, BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZaoQiuEffect", "早秋中秋晚秋：三阶段循环增益"));
            tooltips.Add(new TooltipLine(Mod, "ZaoQiuPhase1", "早秋：+10%伤害，+5%真元恢复（丰收之始）"));
            tooltips.Add(new TooltipLine(Mod, "ZaoQiuPhase2", "中秋：+15%伤害，+10%暴击（丰收之巅）"));
            tooltips.Add(new TooltipLine(Mod, "ZaoQiuPhase3", "晚秋：+10%防御，+5生命/秒恢复（蓄冬之备）"));
            tooltips.Add(new TooltipLine(Mod, "ZaoQiuDuration", $"持续：{BuffDuration / 60}秒（每阶段{200 / 60}秒）"));
            tooltips.Add(new TooltipLine(Mod, "ZaoQiuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
