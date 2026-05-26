using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转暗道功能蛊", "一转", "暗")]
    public class 夜壶 : ModItem
    {
        private const int QiCostPerUse = 3;
        private const int PoisonDuration = 180;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 30;
            Item.value = 500;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
            Item.useTurn = true;
            Item.noMelee = true;
            Item.shootSpeed = 8f;
            Item.shoot = ModContent.ProjectileType<YeHuProj>();
            Item.damage = 5;
            Item.knockBack = 1f;
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

            Vector2 baseVel = Vector2.Normalize(Main.MouseWorld - player.Center) * Item.shootSpeed;
            float spread = MathHelper.ToRadians(15f);

            for (int i = -1; i <= 1; i++)
            {
                float angle = baseVel.ToRotation() + spread * i;
                Vector2 vel = new Vector2((float)System.Math.Cos(angle), (float)System.Math.Sin(angle)) * Item.shootSpeed;

                Projectile.NewProjectile(
                    player.GetSource_ItemUse(Item),
                    player.Center + vel.SafeNormalize(Vector2.Zero) * 20f,
                    vel,
                    ModContent.ProjectileType<YeHuProj>(),
                    Item.damage,
                    Item.knockBack,
                    player.whoAmI
                );
            }

            for (int i = 0; i < 8; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.CorruptGibs);
                d.noGravity = true;
                d.velocity *= 0.5f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "YeHuEffect", "泼洒恶臭液体，使敌人中毒3秒"));
            tooltips.Add(new TooltipLine(Mod, "YeHuQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
