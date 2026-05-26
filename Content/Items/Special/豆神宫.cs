using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转木道功能蛊屋", "三转", "木")]
    public class 豆神宫 : ModItem
    {
        private const int QiCostPerUse = 20;
        private const int BeanCount = 5;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 10;
            Item.value = 50000;
            Item.consumable = true;
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

            for (int i = 0; i < BeanCount; i++)
            {
                float angle = MathHelper.TwoPi / BeanCount * i + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 spawnOffset = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * 30f;
                Vector2 spawnPos = player.Center + spawnOffset;
                Vector2 vel = spawnOffset.SafeNormalize(Vector2.Zero) * 3f;

                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    spawnPos,
                    vel,
                    ModContent.ProjectileType<DouShenProj>(),
                    20,
                    2f,
                    player.whoAmI
                );
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DouShenGongEffect", "撒豆成兵！召唤5个豆兵为你战斗15秒"));
            tooltips.Add(new TooltipLine(Mod, "DouShenGongDamage", "每个豆兵造成20点伤害"));
            tooltips.Add(new TooltipLine(Mod, "DouShenGongConsumable", "一次性消耗品"));
            tooltips.Add(new TooltipLine(Mod, "DouShenGongQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
