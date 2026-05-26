using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转风道功能蛊屋", "二转", "风")]
    public class 风满楼 : ModItem
    {
        private const int QiCostPerUse = 15;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 30000;
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
                cursorPos,
                Vector2.Zero,
                ModContent.ProjectileType<FengManLouProj>(),
                0,
                0f,
                player.whoAmI
            );

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FengManLouEffect", "在光标位置召唤风之漩涡5秒，将敌人拉向中心"));
            tooltips.Add(new TooltipLine(Mod, "FengManLouQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
