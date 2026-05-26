using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Abstractions;
using VerminLordMod.Common.ImplementationTracker;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Items.GuHouses
{
    /// <summary>
    /// 仙蛊屋黑牢 (XianGuWuHeiLao)
    /// 仙蛊屋，外形如黑色流星，能撞击、干扰敌人，由黑城操纵。
    /// 七转战斗型仙蛊屋，具备强大的撞击和干扰能力。
    /// </summary>
    [ImplStatus(ImplStatus.Implemented, "仙蛊屋黑牢 — 七转战斗型仙蛊屋", "七转", "暗")]
    public class XianGuWuHeiLao : GuHouseItem
    {
        public override int HouseLevel => 7;
        public override GuHouseType HouseType => GuHouseType.Combat;
        public override int ActivationQiCost => 400;
        public override int SustainQiCostPerSecond => 15;
        public override float Range => 600f;
        public override bool IsMobile => true;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Lime;
            Item.value = 600000;
            Item.damage = 200;
            Item.knockBack = 10f;
        }

        public override void OnActivate(Player player)
        {
            base.OnActivate(player);
            player.AddBuff(BuffID.ShadowDodge, 600);
            player.AddBuff(BuffID.Titan, 600);
            Main.NewText("仙蛊屋黑牢展开，黑色流星环绕，随时准备撞击敌人！", Color.DarkRed);
        }
    }
}
