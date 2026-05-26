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
    [ImplStatus(ImplStatus.Implemented, "二转金道辅助蛊", "二转", "金")]
    public class QingTongSheLiGu : ModItem
    {
        private const int QiCostPerUse = 10;
        private const int DefenseIncrease = 2;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 20;
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

            var qingTongPlayer = player.GetModPlayer<QingTongSheLiPlayer>();
            qingTongPlayer.PermanentDefenseBonus += DefenseIncrease;

            CombatText.NewText(player.Hitbox, new Color(205, 127, 50), $"+{DefenseIncrease} 防御", true);

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.5f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "QingTongSheLiEffect", $"永久增加{DefenseIncrease}点防御力"));
            tooltips.Add(new TooltipLine(Mod, "QingTongSheLiQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "QingTongSheLiConsumable", "一次性消耗品"));
        }
    }
}
