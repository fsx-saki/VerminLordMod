using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转阴阳道辅助蛊", "二转", "阴阳")]
    public class GuRouTuanYuanGu : ModItem
    {
        private const int QiCostPerUse = 12;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 30;
            Item.value = 5000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.healLife = 50;
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

            player.Heal(50);

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (player.buffType[i] == BuffID.Bleeding || player.buffType[i] == BuffID.Poisoned)
                {
                    player.DelBuff(i);
                    i--;
                }
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "GuRouEffect", "恢复50点生命值并移除流血和中毒效果"));
            tooltips.Add(new TooltipLine(Mod, "GuRouQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
