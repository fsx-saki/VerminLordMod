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
    [ImplStatus(ImplStatus.Implemented, "六转人道仙蛊", "六转", "人")]
    public class JiangShanRuGuXianGu : ModItem
    {
        private const int QiCostPerUse = 50;
        private const int CooldownFrames = 3600;

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.rare = ItemRarityID.Yellow;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 60;
            Item.useAnimation = 60;
            Item.UseSound = SoundID.Item4;
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

            if (player.HasBuff(ModContent.BuffType<JiangShanRuGuCDBuff>()))
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.statLife = player.statLifeMax2;
            qiResource.QiCurrent = qiResource.QiMaxCurrent;

            for (int i = 0; i < 323; i++)
            {
                if (Main.debuff[i] && player.buffTime[i] > 0)
                {
                    if (!BuffID.Sets.NurseCannotRemoveDebuff[i])
                    {
                        player.buffTime[i] = 0;
                    }
                }
            }

            player.AddBuff(ModContent.BuffType<JiangShanRuGuCDBuff>(), CooldownFrames);

            for (int i = 0; i < 30; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PortalBoltTrail);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1f, 2f);
                d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-4f, -1f));
            }

            CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(255, 215, 0), "江山如故！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JiangShanEffect", "恢复全部生命与真元，清除所有可移除的减益效果"));
            tooltips.Add(new TooltipLine(Mod, "JiangShangCD", $"冷却：{CooldownFrames / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "JiangShanQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
