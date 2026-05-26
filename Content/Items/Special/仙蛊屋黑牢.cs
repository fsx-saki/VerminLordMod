using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToEnemy;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转暗道功能蛊屋", "三转", "暗")]
    public class 仙蛊屋黑牢 : ModItem
    {
        private const int QiCostPerUse = 20;
        private const int TrapRange = 400;
        private const int TrapDuration = 180;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 5;
            Item.value = 100000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.noMelee = true;
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

            NPC nearest = null;
            float nearestDist = TrapRange;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly)
                {
                    float dist = Vector2.Distance(player.Center, npc.Center);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        nearest = npc;
                    }
                }
            }

            if (nearest != null)
            {
                nearest.AddBuff(ModContent.BuffType<HeiLaoDebuff>(), TrapDuration);

                for (int i = 0; i < 10; i++)
                {
                    var d = Dust.NewDustDirect(nearest.position, nearest.width, nearest.height, DustID.Shadowflame);
                    d.noGravity = true;
                    d.velocity *= 0.5f;
                    d.scale = Main.rand.NextFloat(1.0f, 1.5f);
                }
            }

            return true;
        }
    }
}
