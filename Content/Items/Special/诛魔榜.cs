using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToEnemy;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转战道功能蛊屋", "三转", "战")]
    public class 诛魔榜 : ModItem
    {
        private const int QiCostPerUse = 18;
        private const int DebuffDuration = 600;
        private const int MarkCount = 3;

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
                Text.ShowTextRed(player, "真元不足，无法催动诛魔榜");
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

            var targets = Main.npc
                .Where(n => n.active && !n.friendly && Vector2.Distance(n.Center, player.Center) <= 2000f)
                .OrderBy(n => Vector2.Distance(n.Center, player.Center))
                .Take(MarkCount)
                .ToList();

            foreach (NPC npc in targets)
            {
                npc.AddBuff(ModContent.BuffType<ZhuMoDebuff>(), DebuffDuration);

                for (int i = 0; i < 8; i++)
                {
                    Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height,
                        DustID.CrimsonTorch, 0f, 0f);
                    dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.8f, 1.3f);
                }
            }

            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                    DustID.CrimsonTorch, 0f, 0f);
                dust.velocity = Main.rand.NextVector2Circular(3f, 3f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            Text.ShowTextGreen(player, $"诛魔榜：标记了{targets.Count}个敌人！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZhuMoDesc", "三转战道功能蛊屋：诛魔榜"));
            tooltips.Add(new TooltipLine(Mod, "ZhuMoEffect", "标记最近3个敌人，使其受到+20%伤害，持续10秒"));
            tooltips.Add(new TooltipLine(Mod, "ZhuMoQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
