using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转空间道功能蛊", "三转", "空间")]
    public class DouKongGu : ModItem
    {
        private const int QiCostPerUse = 25;
        private const float MaxTeleportRange = 800f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 15000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item8;
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

            Vector2 targetPos = Main.MouseWorld;
            Vector2 direction = targetPos - player.Center;

            if (direction.Length() > MaxTeleportRange)
            {
                direction.Normalize();
                targetPos = player.Center + direction * MaxTeleportRange;
            }

            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(player.Center, 0, 0, DustID.Teleporter);
                d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }

            player.Center = targetPos;
            player.velocity = Vector2.Zero;
            player.AddBuff(BuffID.ShadowDodge, 30);

            for (int i = 0; i < 15; i++)
            {
                var d = Dust.NewDustDirect(player.Center, 0, 0, DustID.Teleporter);
                d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DouKongEffect", "斗空：瞬移至鼠标位置（最大距离800像素）"));
            tooltips.Add(new TooltipLine(Mod, "DouKongEffect2", "瞬移后获得0.5秒无敌"));
            tooltips.Add(new TooltipLine(Mod, "DouKongQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
