using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转炼道功能蛊", "二转", "炼")]
    public class FangWeiGu : ModItem
    {
        private const int QiCostPerUse = 12;
        private const int RevealDuration = 600;
        private const int SpelunkerDuration = 300;

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

            int buffType = ModContent.BuffType<FangWeiBuff>();
            player.AddBuff(buffType, RevealDuration);
            player.AddBuff(BuffID.Spelunker, SpelunkerDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FangWeiEffect", "仿伪：揭示所有隐身敌人10秒，探矿5秒（火眼金睛辨真伪）"));
            tooltips.Add(new TooltipLine(Mod, "FangWeiDuration", $"揭示持续：{RevealDuration / 60}秒 / 探矿持续：{SpelunkerDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "FangWeiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
