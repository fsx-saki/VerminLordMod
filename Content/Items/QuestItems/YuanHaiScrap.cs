using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.QuestItems
{
    public class YuanHaiScrap : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 26; Item.height = 24; Item.maxStack = 1; Item.value = 0;
            Item.rare = ItemRarityID.Gray;
            Item.damage = 6; Item.DamageType = DamageClass.Melee; Item.knockBack = 2f;
            Item.useTime = 20; Item.useAnimation = 20; Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1; Item.autoReuse = false;
        }

        public override string Texture => "Terraria/Images/Item_" + ItemID.TinCan;

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            TooltipLine nameLine = tooltips.Find(l => l.Name == "ItemName");
            if (nameLine != null) nameLine.Text = "破金属片";
            tooltips.Add(new TooltipLine(Mod, "Desc", "锋利的金属片，是某种劣质工业品的外壳，小心割伤。"));
        }
    }
}
