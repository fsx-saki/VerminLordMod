using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
    [ImplStatus(ImplStatus.Implemented, "三转成败道辅助蛊", "三转", "成败")]
    public class YiSiYiShengGu : ModItem
    {
        private const int QiCostPerUse = 20;

        public static LocalizedText UsesXQiText { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(QiCostPerUse)));
            tooltips.Add(new TooltipLine(Mod, "YiSiYiShengDesc", "一死一生蛊 — 死生相循，破而后立"));
            tooltips.Add(new TooltipLine(Mod, "YiSiYiShengEffect", "暂时「死亡」后浴火重生：HP降至1并眩晕30帧，随后恢复50%生命并+25%伤害持续300帧"));
            tooltips.Add(new TooltipLine(Mod, "YiSiYiShengRestriction", "生命值低于30%时无法使用"));
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            float hpRatio = (float)player.statLife / player.statLifeMax2;
            return qiResource.QiCurrent >= QiCostPerUse && hpRatio >= 0.30f;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.AddBuff(ModContent.BuffType<YiSiYiShengBuff>(), 330);

            return true;
        }
    }
}
