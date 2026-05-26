using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转变化道功能蛊屋", "三转", "变")]
    public class 搬拦亭 : ModItem
    {
        private const int QiCostPerUse = 20;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 50000;
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

            Vector2 cursorPos = Main.MouseWorld;
            Projectile.NewProjectile(
                player.GetSource_FromThis(),
                cursorPos.X, cursorPos.Y,
                0f, 0f,
                ModContent.ProjectileType<BanLanTingProj>(),
                0, 0f, player.whoAmI
            );

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "BanLanEffect", "在光标位置创建屏障，阻挡敌方弹幕10秒"));
            tooltips.Add(new TooltipLine(Mod, "BanLanQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
