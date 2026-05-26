using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转光道辅助蛊", "二转", "光")]
    public class ZhouMaoGu : ModItem
    {
        private const int QiCostPerUse = 10;
        private const int BuffDuration = 480;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 8000;
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
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int buffType = ModContent.BuffType<ZhouMaoBuff>();
            player.AddBuff(buffType, BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZhouMaoEffect", "昼貌蛊：白天伤害+12%、防御+8；夜间伤害+5%"));
            tooltips.Add(new TooltipLine(Mod, "ZhouMaoEffect2", "昼夜均发光"));
            tooltips.Add(new TooltipLine(Mod, "ZhouMaoDuration", $"持续：{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "ZhouMaoQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
