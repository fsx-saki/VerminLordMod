using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转运道辅助仙蛊", "五转", "运")]
    public class YingYunXianGu : ModItem
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
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(28)));
            tooltips.Add(new TooltipLine(Mod, "YingYunDesc", "应运仙蛊 — 顺应天运，福运降临"));
            tooltips.Add(new TooltipLine(Mod, "YingYunEffect", "+10%暴击，+10%闪避，+15%金币掉落，10%概率双倍伤害"));
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= 28;
        }

        public override bool? UseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(28);

            player.AddBuff(ModContent.BuffType<YingYunBuff>(), 600);

            return true;
        }
    }
}
