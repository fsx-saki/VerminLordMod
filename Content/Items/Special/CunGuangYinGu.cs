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
    [ImplStatus(ImplStatus.Implemented, "一转宙道功能蛊", "一转", "宙")]
    public class CunGuangYinGu : ModItem
    {
        private const int QiCostPerUse = 10;
        private const int SlowDuration = 300;
        private const float SlowRadius = 600f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 1;
            Item.value = 5000;
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

            int debuffType = ModContent.BuffType<CunGuangYinDebuff>();

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && Vector2.Distance(player.Center, npc.Center) <= SlowRadius)
                {
                    npc.AddBuff(debuffType, SlowDuration);
                }
            }

            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi / 20f * i;
                Vector2 dir = angle.ToRotationVector2();
                var d = Dust.NewDustDirect(player.Center + dir * 30f, 0, 0, DustID.MagicMirror);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
                d.velocity = dir * 3f;
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "CunGuangYinEffect", "寸光阴：使周围敌人减速30%，持续5秒"));
            tooltips.Add(new TooltipLine(Mod, "CunGuangYinRange", $"范围：{(int)(SlowRadius / 16)}格"));
            tooltips.Add(new TooltipLine(Mod, "CunGuangYinQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
