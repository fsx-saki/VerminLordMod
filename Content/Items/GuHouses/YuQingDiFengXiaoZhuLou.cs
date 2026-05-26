using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Abstractions;
using VerminLordMod.Common.ImplementationTracker;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Items.GuHouses
{
    /// <summary>
    /// 玉清滴风小竹楼 (YuQingDiFengXiaoZhuLou)
    /// 八转仙蛊屋，以风道仙蛊为核心构建，轻盈飘逸，攻防一体。
    /// 具备强大的风道攻击能力和移动速度加成。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "玉清滴风小竹楼 — 八转仙蛊屋", "八转", "风")]
    public class YuQingDiFengXiaoZhuLou : GuHouseItem
    {
        public override int HouseLevel => 8;
        public override GuHouseType HouseType => GuHouseType.Composite;
        public override int ActivationQiCost => 800;
        public override int SustainQiCostPerSecond => 30;
        public override float Range => 800f;
        public override bool IsMobile => true;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 30;
            Item.height = 30;
            Item.rare = ItemRarityID.Cyan;
            Item.value = 1500000;
            Item.damage = 400;
            Item.knockBack = 12f;
        }

        public override void OnActivate(Player player)
        {
            base.OnActivate(player);
            player.AddBuff(BuffID.WindPushed, 600);
            player.AddBuff(BuffID.Swiftness, 600);
            Main.NewText("玉清滴风小竹楼展开，清风环绕，身法如风！", Color.Cyan);
        }
    }
}
