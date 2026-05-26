using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转人道辅助仙蛊", "五转", "人")]
    public class YiHouXianGu : ModItem
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
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 500000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(25)));
            tooltips.Add(new TooltipLine(Mod, "YiHouDesc", "使用后储存遗后之力，生命值低于25%时自动激活"));
            tooltips.Add(new TooltipLine(Mod, "YiHouEffect", "激活：回复50生命值，获得300帧+20%伤害加成"));
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            var yiHouPlayer = player.GetModPlayer<YiHouPlayer>();

            if (yiHouPlayer.HasLegacy)
            {
                return false;
            }

            return qiResource.QiCurrent >= 25;
        }

        public override bool? UseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            var yiHouPlayer = player.GetModPlayer<YiHouPlayer>();

            qiResource.ConsumeQi(25);
            yiHouPlayer.HasLegacy = true;

            CombatText.NewText(player.getRect(), new Microsoft.Xna.Framework.Color(255, 215, 0), "遗后之力已储存！");

            return true;
        }
    }
}
