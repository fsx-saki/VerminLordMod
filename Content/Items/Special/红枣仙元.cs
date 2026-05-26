using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转木道辅助仙蛊", "五转", "木")]
    public class 红枣仙元 : ModItem
    {
        private const int QiCostPerUse = 25;
        private const int HealAmount = 100;
        private const int RegenBuffDuration = 600;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 10;
            Item.value = 50000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item3;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.healLife = HealAmount;
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

            player.statLife += HealAmount;
            if (player.statLife > player.statLifeMax2)
                player.statLife = player.statLifeMax2;
            player.HealEffect(HealAmount);

            player.AddBuff(BuffID.Regeneration, RegenBuffDuration);

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.JungleGrass);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.velocity.Y -= 1f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HongZaoEffect", $"恢复{HealAmount}点生命值，并在10秒内增加生命回复速度"));
            tooltips.Add(new TooltipLine(Mod, "HongZaoQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "HongZaoConsumable", "一次性消耗品"));
        }
    }
}
