using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转奴道功能蛊屋", "一转", "奴")]
    public class 鸡笼犬舍 : ModItem
    {
        private const int QiCostPerUse = 5;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
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

            int buffType = ModContent.BuffType<JiLongQuanSheBuff>();
            player.AddBuff(buffType, 300);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JiLongQuanSheEffect", "召唤2只兔子和1只鸟跟随你"));
            tooltips.Add(new TooltipLine(Mod, "JiLongQuanSheMinion", "召唤物伤害+5%"));
            tooltips.Add(new TooltipLine(Mod, "JiLongQuanSheDuration", "持续5秒"));
            tooltips.Add(new TooltipLine(Mod, "JiLongQuanSheQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
