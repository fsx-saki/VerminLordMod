using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "奴道功能蛊", "一至三转", "奴")]
    public class YuYuGu : ModItem
    {
        private const int QiCost = 10;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 20;
            Item.value = 10000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCost;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (!qiResource.ConsumeQi(QiCost))
            {
                Text.ShowTextRed(player, "真元不足，无法催动驭鱼蛊");
                return false;
            }

            Projectile.NewProjectile(
                player.GetSource_FromThis(),
                player.Center,
                player.DirectionTo(Main.MouseWorld) * 6f,
                ModContent.ProjectileType<YuYuFishProj>(),
                20,
                2f,
                player.whoAmI
            );

            Text.ShowTextGreen(player, "驭鱼蛊催动！召唤驭鱼攻击敌人");
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "YuYuGuDesc", "奴道功能蛊：召唤一条驭鱼，自动追踪攻击附近敌人"));
            tooltips.Add(new TooltipLine(Mod, "YuYuGuDuration", "驭鱼持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "YuYuGuQiCost", $"[c/BB5BC9:真元消耗: {QiCost}]"));
            tooltips.Add(new TooltipLine(Mod, "YuYuGuConsumable", "[c/ff6600:一次性消耗蛊]"));
        }
    }
}
