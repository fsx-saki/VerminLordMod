using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转梦道功能蛊屋", "三转", "梦")]
    public class 幻景园 : ModItem
    {
        private const int QiCostPerUse = 20;
        private const int BuffDuration = 600;
        private const int CloneCount = 3;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 50000;
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
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            player.AddBuff(ModContent.BuffType<HuanJingBuff>(), BuffDuration);

            for (int i = 0; i < CloneCount; i++)
            {
                float angle = MathHelper.TwoPi / CloneCount * i;
                Vector2 spawnOffset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 80f;
                Vector2 spawnPos = player.Center + spawnOffset;

                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    spawnPos,
                    Vector2.Zero,
                    ModContent.ProjectileType<HuanJingCloneProj>(),
                    0,
                    0f,
                    player.whoAmI
                );
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HuanJingEffect", "创造3个幻影分身迷惑敌人10秒"));
            tooltips.Add(new TooltipLine(Mod, "HuanJingDodge", "敌人有30%概率攻击落空"));
            tooltips.Add(new TooltipLine(Mod, "HuanJingQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
