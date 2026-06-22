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

        private int _updateTimer = 0;

        public override void OnUpdate(Player player)
        {
            // 每 60 帧（约 1 秒）生成环绕黑色流星弹幕
            _updateTimer++;
            if (_updateTimer >= 60)
            {
                _updateTimer = 0;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var source = player.GetSource_FromThis();
                    int count = 3 + HouseLevel; // 10 颗流星环绕
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi / count * i + Main.GameUpdateCount * 0.02f;
                        Vector2 orbitPos = player.Center + angle.ToRotationVector2() * 120f;
                        Vector2 velocity = (player.Center - orbitPos).SafeNormalize(Vector2.Zero) * 2f;
                        // 使用现有弹幕：Meteor1 作为黑色流星占位
                        Projectile.NewProjectile(source, orbitPos, velocity, ProjectileID.Meteor1, Item.damage / 5, 2f, player.whoAmI);
                    }
                }
            }

            // 持续刷新 ShadowDodge（只要仙蛊屋运转，闪避效果不断）
            if (player.buffTime[player.FindBuffIndex(BuffID.ShadowDodge)] < 60)
                player.AddBuff(BuffID.ShadowDodge, 120);
        }

        public override void OnDeactivate(Player player)
        {
            Main.NewText("黑牢收束，流星消散。", Color.DarkRed);
            _updateTimer = 0;
        }
    }
}
