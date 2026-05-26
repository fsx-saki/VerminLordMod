using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转成败道功能蛊", "一转", "成败")]
    public class GuoDeQu : ModItem
    {
        private const int QiCostPerUse = 10;
        private const int HealAmount = 30;
        private const int ResistanceDuration = 180;
        private const float DamageReduction = 0.30f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 15;
            Item.value = 2000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.healLife = HealAmount;
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

            player.Heal(HealAmount);
            player.AddBuff(BuffID.Endurance, ResistanceDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "GuoDeQuEffect", $"过得去：回复{HealAmount}生命，获得3秒30%伤害减免（总会过去的）"));
            tooltips.Add(new TooltipLine(Mod, "GuoDeQuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
