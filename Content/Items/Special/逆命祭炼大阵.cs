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
    public class 逆命祭炼大阵 : ModItem
    {
        private const int QiCostPerUse = 40;
        private const int BuffDuration = 600;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 200000;
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
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;

            if (player.statLife < (int)(player.statLifeMax2 * 0.3f))
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int hpCost = player.statLife / 2;
            player.statLife -= hpCost;
            if (player.statLife < 1)
                player.statLife = 1;

            player.AddBuff(ModContent.BuffType<NiMingBuff>(), BuffDuration);

            CombatText.NewText(player.Hitbox, Microsoft.Xna.Framework.Color.Red, $"-{hpCost}HP", true, false);

            for (int i = 0; i < 20; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.LifeDrain);
                d.noGravity = true;
                d.velocity *= 1.5f;
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "NiMingEffect1", "献祭当前50%生命值，获得伤害+50%、暴击+30%、攻速+20%"));
            tooltips.Add(new TooltipLine(Mod, "NiMingEffect2", "生命值低于30%时无法使用（会致死）"));
            tooltips.Add(new TooltipLine(Mod, "NiMingDuration", "持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "NiMingQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
