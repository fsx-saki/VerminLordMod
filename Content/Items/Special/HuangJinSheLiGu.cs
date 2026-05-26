using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转金道辅助蛊", "三转", "金")]
    public class HuangJinSheLiGu : ModItem
    {
        private const int QiCostPerUse = 18;
        private const int QiMaxIncrease = 15;

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

            CombatText.NewText(player.Hitbox, new Color(255, 215, 0), $"+{QiMaxIncrease} 真元上限", true);

            for (int i = 0; i < 10; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.5f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HuangJinEffect", $"永久增加{QiMaxIncrease}点真元上限"));
            tooltips.Add(new TooltipLine(Mod, "HuangJinQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "HuangJinConsumable", "一次性消耗品"));
        }
    }
}
