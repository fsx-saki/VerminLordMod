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
    public class 万里丝廊 : ModItem
    {
        private const int QiCostPerUse = 18;
        private const int WebCount = 5;

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
            Item.UseSound = SoundID.Item17;
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

            for (int i = 0; i < WebCount; i++)
            {
                float angle = MathHelper.TwoPi / WebCount * i + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Main.rand.NextFloat(6f, 10f);
                Vector2 velocity = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * speed;

                Projectile.NewProjectile(
                    player.GetSource_FromThis(),
                    player.Center,
                    velocity,
                    ModContent.ProjectileType<WanLiSiLangProj>(),
                    15,
                    2f,
                    player.whoAmI
                );
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "WanLiSiLangEffect1", "发射5个蛛丝弹幕，粘附在表面"));
            tooltips.Add(new TooltipLine(Mod, "WanLiSiLangEffect2", "触碰蛛丝的敌人将被减速"));
            tooltips.Add(new TooltipLine(Mod, "WanLiSiLangQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
