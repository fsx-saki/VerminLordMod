using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转成败道功能蛊屋", "三转", "成败")]
    public class 一缺抱憾亭 : ModItem
    {
        private const int QiCostPerUse = 18;
        private const int BuffDuration = 600;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
            Item.useTurn = true;
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

            var yiQuePlayer = player.GetModPlayer<YiQuePlayer>();
            yiQuePlayer.FlawType = Main.rand.Next(1, 5);

            player.AddBuff(ModContent.BuffType<YiQueBuff>(), BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "YiQueEffect1", "10秒内全属性+20%，但随机一项属性-30%（一缺）"));
            tooltips.Add(new TooltipLine(Mod, "YiQueEffect2", "一缺：伤害/防御/速度/暴击 随机削弱"));
            tooltips.Add(new TooltipLine(Mod, "YiQueQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
