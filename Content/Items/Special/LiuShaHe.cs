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
    [ImplStatus(ImplStatus.Implemented, "三转泥道功能蛊屋", "三转", "泥")]
    public class LiuShaHe : ModItem
    {
        private const int QiCostPerUse = 18;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 10000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.shootSpeed = 0f;
            Item.shoot = ModContent.ProjectileType<LiuShaHeProj>();
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
            Projectile.NewProjectile(
                player.GetSource_FromThis(),
                targetPos,
                Microsoft.Xna.Framework.Vector2.Zero,
                ModContent.ProjectileType<LiuShaHeProj>(),
                5,
                0f,
                player.whoAmI
            );

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "LiuShaHeEffect", "流沙河：在光标处创建流沙区域，减速敌人40%，每秒造成5点伤害，持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "LiuShaHeQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
