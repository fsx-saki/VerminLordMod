using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Projectiles;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转奴道功能蛊", "二转", "奴")]
    public class YinLangGu : ModItem
    {
        private const int QiCostPerUse = 15;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 10;
            Item.value = 5000;
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

            Vector2 spawnPos = player.Center + new Vector2(Main.rand.Next(-30, 30), -40f);
            int damage = (int)(player.HeldItem.damage * 0.5f) + 15;
            Projectile.NewProjectile(
                new EntitySource_ItemUse(player, Item),
                spawnPos,
                new Vector2(Main.rand.NextFloat(-2, 2), -3f),
                ModContent.ProjectileType<YinLangProj>(),
                damage,
                3f,
                player.whoAmI
            );

            CombatText.NewText(player.Hitbox, new Color(180, 180, 200), "引狼！");

            for (int i = 0; i < 12; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Bone);
                d.velocity = new Vector2(Main.rand.NextFloat(-3, 3), Main.rand.NextFloat(-4, -1));
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "YinLangEffect", "召唤一只狼影为你战斗15秒"));
            tooltips.Add(new TooltipLine(Mod, "YinLangConsumable", "[c/ff6600:一次性消耗蛊]"));
            tooltips.Add(new TooltipLine(Mod, "YinLangQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
