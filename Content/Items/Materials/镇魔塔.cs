using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 矿石 — 镇魔塔
    /// 铁家在万程山巅建造的百丈高塔，用于关押魔道蛊师，是正道象征。
    /// </summary>
    public class 镇魔塔 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 999;
            Item.value = 2000;
        }
    }
}
