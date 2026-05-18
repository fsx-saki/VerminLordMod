using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 材料 — 红莲/鬼脸
    /// 在光阴长河中出现的鬼脸和红莲，修复了春秋蝉并帮助方源重生。
    /// </summary>
    public class 红莲鬼脸 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 999;
            Item.value = 500;
        }
    }
}
