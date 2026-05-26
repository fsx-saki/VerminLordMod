using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转人道功能蛊", "一转", "人")]
    public class YingShengChong : ModItem
    {
        public static LocalizedText UsesXQiText { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 30;
            Item.value = 1000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 17;
            Item.useTime = 17;
            Item.UseSound = SoundID.Item3;
            Item.consumable = true;
            Item.healLife = 20;
            Item.potion = false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(5)));
            tooltips.Add(new TooltipLine(Mod, "YingShengDesc", "迎生虫 — 迎接新生命，回复20生命值"));
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= 5;
        }

        public override bool? UseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(5);

            player.statLife += 20;
            player.HealEffect(20);

            return true;
        }
    }
}
