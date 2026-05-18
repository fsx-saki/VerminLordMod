using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Items.Materials
{
    /// <summary>
    /// 荒兽资源 — 泥沼蟹
    /// 巨大如山的荒兽，第六次地灾中出现，拥有强大的甲壳和繁殖能力。后被荡魂山杀死，尸体被小狐仙分解。
    /// </summary>
    public class 泥沼蟹 : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 10;
            Item.value = 20000;
        }
    }
}
