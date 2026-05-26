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
    [ImplStatus(ImplStatus.Implemented, "二转人道辅助蛊", "二转", "人")]
    public class YiXueGu : ModItem
    {
        private const int QiCostPerUse = 10;

        public static LocalizedText UsesXQiText { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 2000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(QiCostPerUse)));
            tooltips.Add(new TooltipLine(Mod, "YiXueDesc", "医血蛊 — 以血为引，妙手回春"));
            tooltips.Add(new TooltipLine(Mod, "YiXueEffect", "恢复40生命值，并获得生命回复增益300帧"));
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

            player.HealEffect(40);
            player.statLife += 40;
            if (player.statLife > player.statLifeMax2)
                player.statLife = player.statLifeMax2;

            player.AddBuff(BuffID.Regeneration, 300);

            return true;
        }
    }
}
