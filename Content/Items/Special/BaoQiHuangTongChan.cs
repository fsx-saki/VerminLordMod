using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转气道防御蛊", "三转", "气")]
    class BaoQiHuangTongChan : GuAccessoryItem
    {
        protected override int qiCost => 15;
        protected override int _guLevel => 3;

        private bool _qiAboveThreshold;

        public static LocalizedText UsesXQiText { get; private set; }
        public static LocalizedText ControlRate { get; private set; }
        public static LocalizedText GuLevel { get; private set; }

        public override void SetStaticDefaults()
        {
            UsesXQiText = this.GetLocalization("UsesXQi");
            ControlRate = this.GetLocalization("ControlRate");
            GuLevel = this.GetLocalization("GuLevel");
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Insert(2, new TooltipLine(Mod, "QiCost", UsesXQiText.Format(qiCost)));
            tooltips.Insert(3, new TooltipLine(Mod, "GuLevel", GuLevel.Format(_guLevel)));
            if (controlRate > 0f)
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", ControlRate.Format(controlRate)));
            }
            else
            {
                tooltips.Add(new TooltipLine(Mod, "ControlRate", "右键使用开始炼化"));
            }
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 10000;
            Item.accessory = true;
            Item.defense = 8;
            Item.useStyle = ItemUseStyleID.Guitar;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiMaxCurrent -= qiCost;
            qiResource.ExtraQiRegen += qiResource.BaseQiRegenRate * 0.1f;

            _qiAboveThreshold = qiResource.QiMaxCurrent > 0 &&
                qiResource.QiCurrent / qiResource.QiMaxCurrent >= 0.8f;

            if (_qiAboveThreshold)
            {
                player.statDefense += 5;
            }
        }
    }
}
