using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转战道功能蛊屋", "三转", "战")]
    public class 角连营 : ModItem
    {
        private const int QiCostPerUse = 18;
        private const int BuffDuration = 600;

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

            player.AddBuff(ModContent.BuffType<JiaoLianBuff>(), BuffDuration);

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || i == player.whoAmI)
                    continue;

                float dist = Microsoft.Xna.Framework.Vector2.Distance(player.Center, other.Center);
                if (dist <= 500f)
                {
                    other.AddBuff(ModContent.BuffType<JiaoLianAllyBuff>(), BuffDuration);
                }
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JiaoLianEffect1", "激活期间防御+8，伤害+10%"));
            tooltips.Add(new TooltipLine(Mod, "JiaoLianEffect2", "附近500像素内队友获得防御+5"));
            tooltips.Add(new TooltipLine(Mod, "JiaoLianDuration", "持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "JiaoLianQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
