using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "四转空间道功能蛊屋", "四转", "空间")]
    public class 太宇寺 : ModItem
    {
        private const int QiCostPerUse = 30;
        private const int HealAmount = 50;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item6;
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

            player.Heal(HealAmount);

            for (int i = 0; i < Player.MaxBuffs; i++)
            {
                if (player.buffTime[i] > 0)
                {
                    int buffType = player.buffType[i];
                    if (Main.debuff[buffType])
                    {
                        player.DelBuff(i);
                        i--;
                    }
                }
            }

            player.Spawn(PlayerSpawnContext.RecallFromItem);

            for (int i = 0; i < 20; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.MagicMirror);
                d.velocity = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TaiYuSiEffect1", "传送回出生点，回复50点生命值"));
            tooltips.Add(new TooltipLine(Mod, "TaiYuSiEffect2", "清除所有减益效果"));
            tooltips.Add(new TooltipLine(Mod, "TaiYuSiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
