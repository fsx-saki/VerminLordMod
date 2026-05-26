using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转天道辅助仙蛊", "五转", "天")]
    public class TianQiXianGu : ModItem
    {
        private const int QiCostPerUse = 25;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
            Item.consumable = false;
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
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            var tianQiPlayer = player.GetModPlayer<TianQiPlayer>();
            if (Main.bloodMoon)
                tianQiPlayer.CurrentWeather = TianQiPlayer.WeatherType.BloodMoon;
            else if (Main.IsItRaining)
                tianQiPlayer.CurrentWeather = TianQiPlayer.WeatherType.Rain;
            else if (Main.dayTime)
                tianQiPlayer.CurrentWeather = TianQiPlayer.WeatherType.Day;
            else
                tianQiPlayer.CurrentWeather = TianQiPlayer.WeatherType.Night;

            int buffType = ModContent.BuffType<TianQiBuff>();
            player.AddBuff(buffType, 600);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TianQiEffect", "操控天气之力，根据天气获得不同增益"));
            tooltips.Add(new TooltipLine(Mod, "TianQiRain", "雨天：+15%伤害，+10%真元恢复"));
            tooltips.Add(new TooltipLine(Mod, "TianQiDay", "白天：+12%伤害，+8%暴击"));
            tooltips.Add(new TooltipLine(Mod, "TianQiNight", "夜晚：+10%暴击，+15%闪避"));
            tooltips.Add(new TooltipLine(Mod, "TianQiBlood", "血月：+25%伤害但+10%受伤"));
            tooltips.Add(new TooltipLine(Mod, "TianQiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
