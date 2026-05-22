using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Abstractions;
using VerminLordMod.Common.ImplementationTracker;

namespace VerminLordMod.Content.Items.GuHouses
{
    /// <summary>
    /// 仙蛊屋雏形 (XianGuWuChuXing)
    /// 方源自创的仙蛊屋雏形，以宙道仙蛊为主，擅长隐匿，在石莲岛被威猛老者摧毁。
    /// 七转仙蛊屋，具备基础的隐匿和移动能力。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, note = "仙蛊屋雏形 — 七转仙蛊屋", plannedTurn = "七转", daoType = "宙")]
    public class XianGuWuChuXing : GuHouseItem
    {
        public override int HouseLevel => 7;
        public override GuHouseType HouseType => GuHouseType.Utility;
        public override int ActivationQiCost => 300;
        public override int SustainQiCostPerSecond => 10;
        public override float Range => 500f;
        public override bool IsMobile => true;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Lime;
            Item.value = 500000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
        }

        public override void OnActivate(Player player)
        {
            base.OnActivate(player);
            player.AddBuff(BuffID.Invisibility, 600);
            player.AddBuff(BuffID.Swiftness, 600);
            Main.NewText("仙蛊屋雏形展开，周围空间扭曲，进入隐匿状态！", Color.Lime);
        }
    }
}
