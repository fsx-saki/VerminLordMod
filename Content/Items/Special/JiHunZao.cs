using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转魂道功能蛊屋", "三转", "魂")]
    public class JiHunZao : ModItem
    {
        private const int QiCostPerUse = 18;
        private const int HpSacrifice = 30;
        private const int BuffDuration = 600;
        private const float DamageBonus = 0.20f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 20000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 35;
            Item.useAnimation = 35;
            Item.UseSound = SoundID.Item14;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostPerUse && player.statLife > HpSacrifice;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.statLife -= HpSacrifice;
            if (player.statLife <= 0)
                player.statLife = 1;

            CombatText.NewText(player.Hitbox, Microsoft.Xna.Framework.Color.OrangeRed, $"-{HpSacrifice}HP");

            player.AddBuff(BuffID.Rage, BuffDuration);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JiHunZaoEffect", $"祭魂灶：献祭{HpSacrifice}HP，伤害+{(int)(DamageBonus * 100)}%，持续{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "JiHunZaoQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
