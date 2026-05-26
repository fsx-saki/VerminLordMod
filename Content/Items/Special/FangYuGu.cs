using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Content.Items.Accessories;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转金道防御蛊", "二转", "金")]
    public class FangYuGu : GuAccessoryItem
    {
        protected override int _guLevel => 2;
        protected override int qiCost => 10;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.statDefense += 5;
            player.endurance += 0.03f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FangYuEffect", "增加5点防御力，减少3%受到的伤害"));
            tooltips.Add(new TooltipLine(Mod, "FangYuQiCost", $"占据真元：{qiCost}"));
        }
    }
}
