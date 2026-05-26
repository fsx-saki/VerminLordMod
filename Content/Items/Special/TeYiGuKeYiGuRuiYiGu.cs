using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转变化道辅助蛊", "三转", "变")]
    public class TeYiGuKeYiGuRuiYiGu : ModItem
    {
        private const int QiCostPerUse = 18;
        private const int BuffDuration = 480;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
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

            int roll = Main.rand.Next(100);
            TeYiBuff.MutationType mutation;
            if (roll < 33)
                mutation = TeYiBuff.MutationType.Special;
            else if (roll < 66)
                mutation = TeYiBuff.MutationType.Variable;
            else
                mutation = TeYiBuff.MutationType.Sharp;

            var teYiPlayer = player.GetModPlayer<TeYiPlayer>();
            teYiPlayer.CurrentMutation = mutation;

            int buffType = ModContent.BuffType<TeYiBuff>();
            player.AddBuff(buffType, BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TeYiEffect", "随机变异：33%+20%伤害 / 33%+20%防御 / 34%+20%暴击"));
            tooltips.Add(new TooltipLine(Mod, "TeYiDuration", $"持续{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "TeYiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
