using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转魅道辅助蛊", "二转", "魅")]
    public class XuQingJiaYiGu : ModItem
    {
        private const int QiCostPerUse = 12;
        private const int BuffDuration = 480;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 3000;
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

            int buffType = ModContent.BuffType<XuQingBuff>();
            player.AddBuff(buffType, BuffDuration);

            var xuQingPlayer = player.GetModPlayer<XuQingPlayer>();
            xuQingPlayer.IsInvisible = true;
            xuQingPlayer.HasDeceptiveStrike = true;

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "XuQingEffect", "虚情假意蛊：隐身，+15%暴击，首次攻击破隐但+30%伤害"));
            tooltips.Add(new TooltipLine(Mod, "XuQingDuration", $"持续：{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "XuQingQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
