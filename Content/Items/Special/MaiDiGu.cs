using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转泥道功能蛊", "二转", "泥")]
    public class MaiDiGu : ModItem
    {
        private const int QiCostPerUse = 15;
        private const int BuffDuration = 300;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
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

            int buffType = ModContent.BuffType<MaiDiBuff>();
            player.AddBuff(buffType, BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "MaiDiEffect", "埋地蛊：潜入地下5秒，隐身且无敌，无法移动或攻击，每秒恢复3HP"));
            tooltips.Add(new TooltipLine(Mod, "MaiDiDuration", $"持续：{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "MaiDiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
