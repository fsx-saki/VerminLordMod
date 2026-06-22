using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Abstractions;
using VerminLordMod.Common.ImplementationTracker;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Items.GuHouses
{
    /// <summary>
    /// 仙蛊屋雏形 (XianGuWuChuXing)
    /// 方源自创的仙蛊屋雏形，以宙道仙蛊为主，擅长隐匿，在石莲岛被威猛老者摧毁。
    /// 七转仙蛊屋，具备基础的隐匿和移动能力。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "仙蛊屋雏形 — 七转仙蛊屋", "七转", "宙")]
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

        private int _updateTimer = 0;

        public override void OnUpdate(Player player)
        {
            // 每 120 帧（约 2 秒）续杯隐匿与加速
            _updateTimer++;
            if (_updateTimer >= 120)
            {
                _updateTimer = 0;
                player.AddBuff(BuffID.Invisibility, 180);
                player.AddBuff(BuffID.Swiftness, 180);
            }

            // 空间扭曲特效：随机生成微量星尘
            if (Main.rand.NextBool(30))
            {
                Vector2 pos = player.Center + Main.rand.NextVector2Circular(80f, 80f);
                Dust.NewDust(pos, 4, 4, DustID.MagicMirror, 0, 0, 100, Color.Lime, 0.8f);
            }
        }

        public override void OnDeactivate(Player player)
        {
            Main.NewText("仙蛊屋雏形崩溃，隐匿解除。", Color.Lime);
            _updateTimer = 0;
        }
    }
}
