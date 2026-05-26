using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "六转人道仙蛊", "六转", "人")]
    public class AiQingXianGu : ModItem
    {
        private const int QiCostPerUse = 60;
        private const int BuffDuration = 600;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Purple;
            Item.maxStack = 1;
            Item.value = 5000000;
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
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int buffType = ModContent.BuffType<AiQingBuff>();
            player.AddBuff(buffType, BuffDuration);

            var aiQingPlayer = player.GetModPlayer<AiQingPlayer>();
            aiQingPlayer.HasLinkedTarget = false;

            float closestDist = 600f;
            int closestIdx = -1;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player other = Main.player[i];
                if (!other.active || i == player.whoAmI || other.dead)
                    continue;
                float dist = Vector2.Distance(player.Center, other.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIdx = i;
                }
            }

            if (closestIdx >= 0)
            {
                aiQingPlayer.LinkedWhoAmI = closestIdx;
                aiQingPlayer.HasLinkedTarget = true;
            }
            else
            {
                closestDist = 600f;
                closestIdx = -1;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || !npc.friendly || npc.townNPC == false)
                        continue;
                    float dist = Vector2.Distance(player.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestIdx = i;
                    }
                }

                if (closestIdx >= 0)
                {
                    aiQingPlayer.LinkedNpcIndex = closestIdx;
                    aiQingPlayer.HasLinkedTarget = true;
                    aiQingPlayer.LinkedIsNpc = true;
                }
            }

            if (!aiQingPlayer.HasLinkedTarget)
            {
                aiQingPlayer.SoloEmpower = true;
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "AiQingEffect", "爱情仙蛊：链接最近友方共享伤害，无友方时+30%伤害与5%吸血"));
            tooltips.Add(new TooltipLine(Mod, "AiQingDuration", $"持续：{BuffDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "AiQingQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
