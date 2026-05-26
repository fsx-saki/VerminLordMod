using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转战道辅助蛊", "三转", "战")]
    public class TuiBeiHuanZhanGu : ModItem
    {
        private const int QiCostPerUse = 15;
        private const int DashDistance = 300;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 20000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            Vector2 mouseDir = Main.MouseWorld - player.Center;
            if (mouseDir.Length() > 0f)
                mouseDir.Normalize();

            Vector2 dashVelocity = -mouseDir * (DashDistance / 15f);
            player.velocity = dashVelocity;

            player.immune = true;
            player.immuneTime = 60;

            int buffType = ModContent.BuffType<TuiBeiBuff>();
            player.AddBuff(buffType, 180);

            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Smoke);
                d.velocity = mouseDir * Main.rand.NextFloat(1f, 4f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
                d.alpha = 100;
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TuiBeiEffect", "向鼠标反方向闪避300像素，获得1秒无敌"));
            tooltips.Add(new TooltipLine(Mod, "TuiBeiCounter", "闪避后3秒内下次攻击+15%伤害（反击加成）"));
            tooltips.Add(new TooltipLine(Mod, "TuiBeiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
