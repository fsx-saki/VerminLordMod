using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转空间道功能蛊", "三转", "空间")]
    public class DunKongGu : ModItem
    {
        private const int QiCostPerUse = 20;
        private const int TeleportDistance = 300;
        private const int InvincibilityFrames = 30;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动遁空蛊");
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            Vector2 direction = Vector2.Normalize(Main.MouseWorld - player.Center);
            if (direction == Vector2.Zero)
                direction = new Vector2(player.direction, 0f);

            Vector2 targetPos = player.Center + direction * TeleportDistance;

            player.Center = targetPos;
            player.fallStart = (int)(player.position.Y / 16f);
            player.immune = true;
            player.immuneTime = InvincibilityFrames;

            for (int i = 0; i < 20; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<Dusts.SpaceDust>(), 0f, 0f);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.4f);
            }

            Text.ShowTextGreen(player, "遁空！瞬移至目标方向");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DunKongDesc", "三转空间道功能蛊：遁空"));
            tooltips.Add(new TooltipLine(Mod, "DunKongEffect", "向鼠标方向瞬移300像素，获得短暂无敌"));
            tooltips.Add(new TooltipLine(Mod, "DunKongWallPass", "可穿越墙壁进行瞬移"));
            tooltips.Add(new TooltipLine(Mod, "DunKongQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
