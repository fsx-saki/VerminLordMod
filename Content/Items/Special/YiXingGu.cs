using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转变化道功能蛊", "二转", "变")]
    public class YiXingGu : ModItem
    {
        private const int QiCostPerUse = 18;
        private const float SwapRange = 400f;
        private const int InvincibleFrames = 30;

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
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;

            NPC nearest = FindNearestEnemy(player);
            return nearest != null;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            NPC nearest = FindNearestEnemy(player);
            if (nearest == null)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            Vector2 playerPos = player.Center;
            Vector2 npcPos = nearest.Center;

            player.Center = npcPos;
            nearest.Center = playerPos;

            player.immune = true;
            player.immuneTime = InvincibleFrames;
            player.immuneNoBlink = false;

            nearest.immune[255] = InvincibleFrames;

            for (int i = 0; i < 20; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.MagicMirror);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            for (int i = 0; i < 20; i++)
            {
                var d = Dust.NewDustDirect(nearest.position, nearest.width, nearest.height, DustID.MagicMirror);
                d.velocity = Main.rand.NextVector2Circular(4f, 4f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, player.position.X, player.position.Y);
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, nearest.whoAmI);

            return true;
        }

        private NPC FindNearestEnemy(Player player)
        {
            NPC nearest = null;
            float minDist = SwapRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(player.Center, npc.Center);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = npc;
                }
            }

            return nearest;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "YiXingEffect", "移形换位：与最近敌人交换位置"));
            tooltips.Add(new TooltipLine(Mod, "YiXingRange", $"范围：{(int)SwapRange}像素"));
            tooltips.Add(new TooltipLine(Mod, "YiXingInvincible", $"交换后双方无敌{InvincibleFrames}帧"));
            tooltips.Add(new TooltipLine(Mod, "YiXingQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
