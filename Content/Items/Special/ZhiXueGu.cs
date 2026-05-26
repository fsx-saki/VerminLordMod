using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转人道功能蛊", "一转", "人")]
    public class ZhiXueGu : ModItem
    {
        private const int QiCostPerUse = 3;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 30;
            Item.value = 500;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.healLife = 15;
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

            player.Heal(15);

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (player.buffType[i] == BuffID.Bleeding)
                {
                    player.buffType[i] = 0;
                    player.buffTime[i] = 0;
                    break;
                }
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZhiXueEffect", "止血蛊：恢复15HP，移除流血状态"));
            tooltips.Add(new TooltipLine(Mod, "ZhiXueQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
