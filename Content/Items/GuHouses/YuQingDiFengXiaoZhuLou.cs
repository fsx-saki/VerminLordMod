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

        private int _updateTimer = 0;

        public override void OnUpdate(Player player)
        {
            // 每 60 帧（约 1 秒）生成风刃弹幕
            _updateTimer++;
            if (_updateTimer >= 60)
            {
                _updateTimer = 0;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var source = player.GetSource_FromThis();
                    // 向鼠标方向发射风刃（BladeOfGrass 作为风刃占位）
                    Vector2 mouseDir = (Main.MouseWorld - player.Center).SafeNormalize(Vector2.UnitX);
                    Vector2 velocity = mouseDir * 10f;
                    Projectile.NewProjectile(source, player.Center, velocity, ProjectileID.BladeOfGrass, Item.damage / 4, 3f, player.whoAmI);
                }
            }

            // 持续刷新风推与加速
            if (player.buffTime[player.FindBuffIndex(BuffID.WindPushed)] < 60)
                player.AddBuff(BuffID.WindPushed, 120);
            if (player.buffTime[player.FindBuffIndex(BuffID.Swiftness)] < 60)
                player.AddBuff(BuffID.Swiftness, 120);
        }

        public override void OnDeactivate(Player player)
        {
            Main.NewText("玉清滴风小竹楼收束，清风止息。", Color.Cyan);
            _updateTimer = 0;
        }
    }
}
