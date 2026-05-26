using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转力道辅助蛊", "二转", "力")]
    public class JingXueGu : ModItem
    {
        private const int QiCostPerUse = 10;
        private const int BuffDuration = 300;
        private const float HpConvertRatio = 0.20f;
        private const float DamageBonus = 0.25f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 10000;
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
            int hpCost = (int)(player.statLife * HpConvertRatio);
            return qiResource.QiCurrent >= QiCostPerUse && player.statLife > hpCost;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int hpCost = (int)(player.statLife * HpConvertRatio);
            player.statLife -= hpCost;
            if (player.statLife <= 0)
                player.statLife = 1;

            CombatText.NewText(player.Hitbox, Color.Crimson, $"-{hpCost}HP");

            player.AddBuff(BuffID.Rage, BuffDuration);
            player.AddBuff(BuffID.Weak, BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JingXueEffect", $"精血蛊：消耗20%当前HP，伤害+{(int)(DamageBonus * 100)}%，持续{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "JingXueQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
