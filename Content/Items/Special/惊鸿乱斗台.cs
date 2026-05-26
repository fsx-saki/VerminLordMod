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
    [ImplStatus(ImplStatus.Implemented, "四转战道功能蛊屋", "四转", "战")]
    public class 惊鸿乱斗台 : ModItem
    {
        private const int QiCostPerUse = 30;
        private const int BuffDuration = 600;

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

            var jingHongPlayer = player.GetModPlayer<JingHongPlayer>();
            jingHongPlayer.CastPosition = player.Center;

            player.AddBuff(ModContent.BuffType<JingHongBuff>(), BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JingHongEffect1", "创造战斗领域10秒：+25%伤害，+15%暴击率，+20%攻击速度"));
            tooltips.Add(new TooltipLine(Mod, "JingHongEffect2", "无法离开施法点400像素范围"));
            tooltips.Add(new TooltipLine(Mod, "JingHongQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
