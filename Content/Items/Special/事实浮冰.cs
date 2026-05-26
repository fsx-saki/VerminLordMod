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
    [ImplStatus(ImplStatus.Implemented, "三转冰雪道功能蛊屋", "三转", "冰雪")]
    public class 事实浮冰 : ModItem
    {
        private const int QiCostPerUse = 15;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item30;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.shoot = ModContent.ProjectileType<ShiShiFuBingProj>();
            Item.shootSpeed = 1f;
            Item.noMelee = true;
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

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ShiShiFuBingEffect", "在光标位置创建浮冰平台，持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "ShiShiFuBingReveal", "揭示浮冰300像素内的隐身敌人"));
            tooltips.Add(new TooltipLine(Mod, "ShiShiFuBingQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
