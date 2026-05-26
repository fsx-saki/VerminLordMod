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
    [ImplStatus(ImplStatus.Implemented, "一转魅道辅助蛊", "一转", "魅")]
    public class FaQingGu : ModItem
    {
        private const int QiCostPerUse = 6;
        private const int BuffDuration = 480;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 1;
            Item.value = 20000;
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
                Text.ShowTextRed(player, "真元不足，无法催动发情蛊");
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

            player.AddBuff(ModContent.BuffType<FaQingBuff>(), BuffDuration);

            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<Dusts.CharmDust>(), 0f, 0f);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }

            Text.ShowTextGreen(player, "发情蛊：活力涌动！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "FaQingDesc", "一转魅道辅助蛊：发情"));
            tooltips.Add(new TooltipLine(Mod, "FaQingEffect", "+10%伤害，+15%移动速度，+5%暴击率，持续8秒"));
            tooltips.Add(new TooltipLine(Mod, "FaQingQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
