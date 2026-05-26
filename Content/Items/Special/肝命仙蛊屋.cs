using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转人道功能蛊屋", "五转", "人")]
    public class 肝命仙蛊屋 : ModItem
    {
        private const int QiCostPerUse = 35;
        private const int BuffDuration = 600;
        private const float HpSacrificeRatio = 0.30f;
        private const float MinHpRatio = 0.40f;

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
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            float hpRatio = (float)player.statLife / player.statLifeMax2;
            return qiResource.QiCurrent >= QiCostPerUse && hpRatio >= MinHpRatio;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int hpSacrifice = (int)(player.statLife * HpSacrificeRatio);
            player.statLife -= hpSacrifice;
            if (player.statLife < 1)
                player.statLife = 1;

            CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(255, 50, 50), $"-{hpSacrifice}", true);

            player.AddBuff(ModContent.BuffType<GanMingBuff>(), BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "GanMingEffect1", "献祭30%当前生命值，获得10秒战斗领域"));
            tooltips.Add(new TooltipLine(Mod, "GanMingEffect2", "领域效果：+30%伤害，+20%暴击率，+15%攻击速度"));
            tooltips.Add(new TooltipLine(Mod, "GanMingWarning", "生命值低于40%时无法使用"));
            tooltips.Add(new TooltipLine(Mod, "GanMingQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
