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
    [ImplStatus(ImplStatus.Implemented, "三转规则道功能蛊", "三转", "规则")]
    public class TianLuoGu : ModItem
    {
        private const int QiCostPerUse = 20;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.shoot = ModContent.ProjectileType<TianLuoNetProj>();
            Item.shootSpeed = 1f;
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
            tooltips.Add(new TooltipLine(Mod, "TianLuoEffect", "在光标处展开天罗地网，持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "TianLuoSlow", "网内敌人减速30%，无法传送或隐身"));
            tooltips.Add(new TooltipLine(Mod, "TianLuoProj", "网内敌方弹幕被摧毁"));
            tooltips.Add(new TooltipLine(Mod, "TianLuoQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
