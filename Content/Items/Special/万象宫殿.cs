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
    [ImplStatus(ImplStatus.Implemented, "五转变化道功能蛊屋", "五转", "变")]
    public class 万象宫殿 : ModItem
    {
        private const int QiCostPerUse = 45;
        private const int BuffDuration = 300;
        private const int ShrinkRange = 600;

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

            player.AddBuff(ModContent.BuffType<WanXiangBuff>(), BuffDuration);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(player.Center, npc.Center);
                if (dist <= ShrinkRange)
                {
                    npc.AddBuff(ModContent.BuffType<WanXiangShrinkDebuff>(), BuffDuration);

                    for (int j = 0; j < 3; j++)
                    {
                        var d = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.PurpleTorch);
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
            tooltips.Add(new TooltipLine(Mod, "WanXiangEffect1", "万象变幻5秒：自身+25%伤害，+10%防御"));
            tooltips.Add(new TooltipLine(Mod, "WanXiangEffect2", "缩小600像素内所有敌人：-25%伤害，-15%防御"));
            tooltips.Add(new TooltipLine(Mod, "WanXiangQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
