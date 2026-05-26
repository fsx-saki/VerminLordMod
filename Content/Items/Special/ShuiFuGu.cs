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
    [ImplStatus(ImplStatus.Implemented, "四转梦道辅助蛊", "四转", "梦")]
    public class ShuiFuGu : ModItem
    {
        private const int QiCost = 20;

        public static LocalizedText UsesXQiText { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(QiCost)));
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = false;
            Item.autoReuse = false;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCost)
            {
                Text.ShowTextRed(player, "真元不足，无法使用睡伏蛊");
                return false;
            }

            if (player.HasBuff(ModContent.BuffType<ShuiFuSleepingBuff>()))
            {
                Text.ShowTextRed(player, "正在沉睡中...");
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCost);

            player.AddBuff(ModContent.BuffType<ShuiFuSleepingBuff>(), 300);

            Text.ShowTextGreen(player, "睡伏蛊发动！进入沉睡恢复状态...");

            return true;
        }
    }
}
