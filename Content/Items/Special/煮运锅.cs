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
    [ImplStatus(ImplStatus.Implemented, "三转运道功能蛊屋", "三转", "运")]
    public class 煮运锅 : ModItem
    {
        private const int QiCostPerUse = 18;
        private const int BuffDuration = 600;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Pink;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item3;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动煮运锅");
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int fortuneType = Main.rand.Next(4);
            var zhuYunPlayer = player.GetModPlayer<ZhuYunPlayer>();
            zhuYunPlayer.FortuneType = fortuneType;

            player.AddBuff(ModContent.BuffType<ZhuYunBuff>(), BuffDuration);

            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                    DustID.GoldFlame, 0f, 0f);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            string fortuneName = fortuneType switch
            {
                0 => "鸿运",
                1 => "守护运",
                2 => "幸运",
                3 => "平衡运",
                _ => "未知"
            };

            Text.ShowTextGreen(player, $"煮运锅：{fortuneName}降临！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZhuYunDesc", "三转运道功能蛊屋：煮运锅"));
            tooltips.Add(new TooltipLine(Mod, "ZhuYunEffect", "随机获得一种运势10秒："));
            tooltips.Add(new TooltipLine(Mod, "ZhuYunEffect1", "  25%鸿运：+20%伤害"));
            tooltips.Add(new TooltipLine(Mod, "ZhuYunEffect2", "  25%守护运：+20%防御"));
            tooltips.Add(new TooltipLine(Mod, "ZhuYunEffect3", "  25%幸运：+20%暴击"));
            tooltips.Add(new TooltipLine(Mod, "ZhuYunEffect4", "  25%平衡运：+8%伤害/防御/暴击"));
            tooltips.Add(new TooltipLine(Mod, "ZhuYunQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
