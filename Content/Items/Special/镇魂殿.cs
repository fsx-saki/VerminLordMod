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
    [ImplStatus(ImplStatus.Implemented, "四转魂道功能蛊屋", "四转", "魂")]
    public class 镇魂殿 : ModItem
    {
        private const int QiCostPerUse = 28;
        private const int BuffDuration = 600;
        private const int AuraRadius = 500;

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
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动镇魂殿");
                return false;
            }
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.AddBuff(ModContent.BuffType<ZhenHunBuff>(), BuffDuration);

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Microsoft.Xna.Framework.Vector2.Distance(npc.Center, player.Center);
                if (dist <= AuraRadius)
                {
                    npc.AddBuff(BuffID.Weak, BuffDuration);
                    npc.GetGlobalNPC<ZhenHunNPC>().ZhenHunWeakened = true;
                }
            }

            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                    DustID.Ghost, 0f, 0f);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            Text.ShowTextGreen(player, "镇魂殿：镇压群魂！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZhenHunDesc", "四转魂道功能蛊屋：镇魂殿"));
            tooltips.Add(new TooltipLine(Mod, "ZhenHunEffect", "周围敌人虚弱(-15%伤害)10秒，自身+15防御，免疫混乱"));
            tooltips.Add(new TooltipLine(Mod, "ZhenHunQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
