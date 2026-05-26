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
    [ImplStatus(ImplStatus.Implemented, "二转盗道功能蛊", "二转", "盗")]
    public class QiangQuGu : ModItem
    {
        private const int QiCostPerUse = 15;

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

            int silverCount = Main.rand.Next(3, 6);
            for (int i = 0; i < silverCount; i++)
            {
                player.QuickSpawnItem(null, ItemID.SilverCoin, 1);
            }
            CombatText.NewText(player.Hitbox, new Color(255, 215, 0), $"窃取{silverCount}银币！");

            NPC nearestEnemy = null;
            float minDist = 600f;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(player.Center, npc.Center);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestEnemy = npc;
                }
            }

            if (nearestEnemy != null && Main.rand.NextFloat() < 0.20f)
            {
                bool stole = false;
                for (int b = 0; b < Player.MaxBuffs; b++)
                {
                    if (nearestEnemy.buffType[b] > 0 && nearestEnemy.buffTime[b] > 0)
                    {
                        int stolenBuffType = nearestEnemy.buffType[b];
                        int stolenBuffTime = nearestEnemy.buffTime[b];
                        nearestEnemy.DelBuff(b);
                        player.AddBuff(stolenBuffType, stolenBuffTime);
                        CombatText.NewText(player.Hitbox, new Color(200, 150, 255), "窃取增益！");
                        stole = true;
                        break;
                    }
                }

                if (!stole)
                {
                    CombatText.NewText(player.Hitbox, new Color(150, 150, 150), "无可窃取");
                }
            }

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 2f;
                d.noGravity = true;
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "QiangQuEffect", "强取：从附近敌人处窃取3-5银币"));
            tooltips.Add(new TooltipLine(Mod, "QiangQuSteal", "20%几率窃取最近敌人的增益效果"));
            tooltips.Add(new TooltipLine(Mod, "QiangQuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
