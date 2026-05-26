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
    [ImplStatus(ImplStatus.Implemented, "五转规则道功能蛊屋", "五转", "规则")]
    public class 十绝大阵 : ModItem
    {
        private const int QiCostPerUse = 40;

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
            Item.UseSound = SoundID.Item4;
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

            int buffType = ModContent.BuffType<ShiJueBuff>();
            player.AddBuff(buffType, 300);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ShiJueEffect", "创建十绝大阵阵域，500像素内敌人每秒受15伤害且防御-20%"));
            tooltips.Add(new TooltipLine(Mod, "ShiJuePlayerBuff", "阵域内自身伤害+20%"));
            tooltips.Add(new TooltipLine(Mod, "ShiJueDuration", "持续5秒"));
            tooltips.Add(new TooltipLine(Mod, "ShiJueQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
