using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转光道功能蛊", "一转", "光")]
    public class GuangYuanGu : ModItem
    {
        private const int QiCostPerUse = 5;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 3000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.shoot = ModContent.ProjectileType<GuangYuanLightProj>();
            Item.shootSpeed = 1f;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            Vector2 cursorPos = Main.MouseWorld;
            Projectile.NewProjectile(source, cursorPos, Vector2.Zero, type, 0, 0f, player.whoAmI);

            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "GuangYuanEffect", "在光标位置创建持久光源（持续10秒）"));
            tooltips.Add(new TooltipLine(Mod, "GuangYuanQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
