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
    [ImplStatus(ImplStatus.Implemented, "二转刀道辅助蛊", "二转", "刀")]
    public class HuiJianGu : ModItem
    {
        private const int QiCostPerUse = 12;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item1;
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

            player.AddBuff(ModContent.BuffType<HuiJianBuff>(), 300);

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.SilverFlame);
                d.velocity *= 0.4f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.2f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HuiJianEffect", "获得悔剑之力：+15%攻击速度，下次攻击必暴击"));
            tooltips.Add(new TooltipLine(Mod, "HuiJianQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
