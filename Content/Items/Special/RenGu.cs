using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Buffs.AddToEnemy;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "八转人道仙蛊-一视同仁", "八转", "人")]
    public class RenGu : ModItem
    {
        private const int QiCostPerUse = 120;
        private const int LinkDuration = 600;
        private const float LinkRange = 500f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动仁蛊");
                return false;
            }

            NPC target = FindNearestEnemy(player.Center, LinkRange);
            if (target == null)
            {
                Text.ShowTextRed(player, "附近没有可链接的目标");
                return false;
            }

            var renGuPlayer = player.GetModPlayer<RenGuPlayer>();
            if (renGuPlayer.IsLinked)
            {
                Text.ShowTextRed(player, "仁蛊链接已存在");
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            NPC target = FindNearestEnemy(player.Center, LinkRange);
            if (target == null)
                return false;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            var renGuPlayer = player.GetModPlayer<RenGuPlayer>();
            renGuPlayer.LinkTarget = target.whoAmI;
            renGuPlayer.LinkTimer = LinkDuration;

            player.AddBuff(ModContent.BuffType<RenGuLinkBuff>(), LinkDuration);
            target.AddBuff(ModContent.BuffType<RenGuLinkedBuff>(), LinkDuration);

            Text.ShowTextGreen(player, $"一视同仁！已与目标建立链接");
            return true;
        }

        private NPC FindNearestEnemy(Vector2 center, float range)
        {
            NPC nearest = null;
            float nearestDist = range;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(center, npc.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }
            return nearest;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "RenGuDesc", "八转人道仙蛊：一视同仁"));
            tooltips.Add(new TooltipLine(Mod, "RenGuEffect", "链接玩家与目标，玩家受到的伤害同步到目标"));
            tooltips.Add(new TooltipLine(Mod, "RenGuHeal", "玩家受到的治疗将同步对目标造成等量伤害"));
            tooltips.Add(new TooltipLine(Mod, "RenGuDuration", $"链接持续：{LinkDuration / 60}秒"));
            tooltips.Add(new TooltipLine(Mod, "RenGuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
