using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转天道辅助蛊", "三转", "天")]
    public class 黄天碎片 : ModItem
    {
        private const int QiCostPerUse = 15;
        private const int QiMaxIncrease = 20;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 10;
            Item.value = 50000;
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

            qiResource.QiMaxBase += QiMaxIncrease;
            qiResource.QiMaxCurrent += QiMaxIncrease;

            CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(255, 220, 50), $"+{QiMaxIncrease} 真元上限", true);

            for (int i = 0; i < 10; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.YellowStarDust);
                d.velocity *= 0.5f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HuangTianEffect", $"永久增加{QiMaxIncrease}点真元上限"));
            tooltips.Add(new TooltipLine(Mod, "HuangTianQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "HuangTianConsumable", "一次性消耗品"));
        }
    }
}
