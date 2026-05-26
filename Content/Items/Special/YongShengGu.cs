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
    [ImplStatus(ImplStatus.Implemented, "概念级人道特殊蛊", "概念级", "人")]
    public class YongShengGu : ModItem
    {
        private const int QiCostPerUse = 200;
        private const int CooldownFrames = 7200;

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.rare = ItemRarityID.Purple;
            Item.maxStack = 1;
            Item.value = 500000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.UseSound = SoundID.Item29;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return false;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;

            if (player.HasBuff(ModContent.BuffType<YongShengCDBuff>()))
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.AddBuff(ModContent.BuffType<YongShengBuff>(), 300);
            player.AddBuff(ModContent.BuffType<YongShengCDBuff>(), CooldownFrames);

            for (int i = 0; i < 40; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PortalBoltTrail);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.5f, 2.5f);
                d.velocity = new Microsoft.Xna.Framework.Vector2(Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-5f, -1f));
            }

            CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(255, 100, 100), "永生！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "YongShengEffect", "5秒内无法死亡，生命值最低为1"));
            tooltips.Add(new TooltipLine(Mod, "YongShengExhaust", "效果结束后虚弱10秒（全属性降低50%）"));
            tooltips.Add(new TooltipLine(Mod, "YongShengCD", $"冷却：{CooldownFrames / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "YongShengQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
