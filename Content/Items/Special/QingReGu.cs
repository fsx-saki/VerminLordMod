using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转水道辅助蛊", "二转", "水")]
    public class QingReGu : ModItem
    {
        private const int QiCostPerUse = 10;
        private const int HealAmount = 15;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
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
            if (player.whoAmI != Main.myPlayer)
                return false;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动清热蛊");
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                int buffType = player.buffType[i];
                if (buffType == BuffID.OnFire || buffType == BuffID.Frostburn ||
                    buffType == BuffID.CursedInferno || buffType == BuffID.OnFire3)
                {
                    player.DelBuff(i);
                    i--;
                }
            }

            player.Heal(HealAmount);

            Text.ShowTextGreen(player, "清热蛊：清除火热，恢复生机！");
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "QingReDesc", "二转水道辅助蛊 — 清热"));
            tooltips.Add(new TooltipLine(Mod, "QingReEffect", "清除着火、霜燃、诅咒焰、暗影焰等debuff"));
            tooltips.Add(new TooltipLine(Mod, "QingReHeal", $"恢复生命：{HealAmount}"));
            tooltips.Add(new TooltipLine(Mod, "QingReQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
