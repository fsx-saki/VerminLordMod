using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "四转木道辅助蛊", "四转", "木")]
    public class TuiChenGu : ModItem
    {
        private const int QiCostPerUse = 30;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 20000;
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

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (player.buffType[i] > 0 && Main.debuff[player.buffType[i]])
                {
                    player.DelBuff(i);
                    i--;
                }
            }

            int buffType = ModContent.BuffType<TuiChenBuff>();
            player.AddBuff(buffType, 300);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TuiChenEffect", "移除所有负面效果，并获得推陈buff：+20%生命回复，+10%伤害，持续5秒"));
            tooltips.Add(new TooltipLine(Mod, "TuiChenQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
