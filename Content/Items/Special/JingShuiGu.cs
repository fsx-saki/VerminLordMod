using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转水道功能蛊", "二转", "水")]
    public class JingShuiGu : ModItem
    {
        private const int QiCostPerUse = 10;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 8000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item21;
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

            if (player.HasBuff(BuffID.Poisoned))
                player.ClearBuff(BuffID.Poisoned);
            if (player.HasBuff(BuffID.Venom))
                player.ClearBuff(BuffID.Venom);
            if (player.HasBuff(BuffID.OnFire))
                player.ClearBuff(BuffID.OnFire);
            if (player.HasBuff(BuffID.CursedInferno))
                player.ClearBuff(BuffID.CursedInferno);

            for (int i = 0; i < 20; i++)
            {
                var d = Dust.NewDustDirect(player.Center, 0, 0, DustID.Water);
                d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-4f, -1f));
                d.noGravity = false;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JingShuiEffect", "净水：清除中毒、猛毒、着火、咒火debuff"));
            tooltips.Add(new TooltipLine(Mod, "JingShuiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
