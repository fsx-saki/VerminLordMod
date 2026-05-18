using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Special
{
    /// <summary>
    /// 特殊物品 — 仙蛊屋雏形
    /// 方源自创的仙蛊屋雏形，以宙道仙蛊为主，擅长隐匿，在石莲岛被威猛老者摧毁。
    /// </summary>
    public class 仙蛊屋雏形 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 100000;
        }
    }
}
