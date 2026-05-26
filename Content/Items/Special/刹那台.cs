using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToEnemy;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转宙道功能蛊屋", "三转", "宙")]
    public class 刹那台 : ModItem
    {
        private const int QiCostPerUse = 25;
        private const int FreezeRange = 400;
        private const int FreezeDuration = 120;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 5;
            Item.value = 50000;
            Item.consumable = true;
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
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(player.Center, npc.Center);
                if (dist <= FreezeRange)
                {
                    npc.AddBuff(ModContent.BuffType<ChaNaStunDebuff>(), FreezeDuration);

                    for (int j = 0; j < 3; j++)
                    {
                        var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.MagicMirror);
                        d.noGravity = true;
                        d.velocity *= 0.2f;
                        d.scale = Main.rand.NextFloat(0.8f, 1.2f);
                    }
                }
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ChaNaEffect", "冻结周围400像素内所有敌人2秒，使其无法移动和攻击"));
            tooltips.Add(new TooltipLine(Mod, "ChaNaQiCost", $"消耗真元：{QiCostPerUse}"));
            tooltips.Add(new TooltipLine(Mod, "ChaNaConsumable", "一次性消耗品"));
        }
    }
}
