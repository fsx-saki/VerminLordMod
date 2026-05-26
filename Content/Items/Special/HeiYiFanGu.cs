using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转奴道功能蛊", "一转", "奴")]
    public class HeiYiFanGu : ModItem
    {
        private const int QiCostPerUse = 5;
        private const int AntCount = 3;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 20;
            Item.value = 500;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return false;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动黑蚁凡蛊");
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

            for (int i = 0; i < AntCount; i++)
            {
                Vector2 position = player.Center + new Vector2(Main.rand.Next(-20, 20), Main.rand.Next(-10, 10));
                Vector2 velocity = Vector2.Normalize(Main.MouseWorld - position) * 6f;
                velocity = velocity.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f));

                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    position,
                    velocity,
                    ModContent.ProjectileType<HeiYiProj>(),
                    8,
                    2f,
                    player.whoAmI
                );
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HeiYiDesc", "一转奴道功能蛊 — 黑蚁"));
            tooltips.Add(new TooltipLine(Mod, "HeiYiEffect", $"释放{AntCount}只黑蚁攻击附近敌人"));
            tooltips.Add(new TooltipLine(Mod, "HeiYiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
