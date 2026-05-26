using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转光道辅助蛊", "一转", "光")]
    public class XiWangGu : ModItem
    {
        private const int QiCostPerUse = 12;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 10;
            Item.value = 5000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
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

            if (player.statLife < player.statLifeMax2 * 0.25f)
            {
                player.Heal(50);
                player.AddBuff(BuffID.ShadowDodge, 300);
            }
            else
            {
                player.Heal(10);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "XiWangEffect", "希望之光：生命值低于25%时恢复50HP并获得5秒无敌"));
            tooltips.Add(new TooltipLine(Mod, "XiWangEffect2", "生命值高于25%时仅恢复10HP"));
            tooltips.Add(new TooltipLine(Mod, "XiWangQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
