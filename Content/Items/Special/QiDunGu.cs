using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "三转气道功能蛊", "三转", "气")]
    public class QiDunGu : ModItem
    {
        private const int QiCostPerUse = 18;
        private const float DashSpeed = 25f;
        private const int InvincibleFrames = 15;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Orange;
            Item.maxStack = 1;
            Item.value = 10000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 25;
            Item.useAnimation = 25;
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

            Vector2 direction = Main.MouseWorld - player.Center;
            if (direction == Vector2.Zero)
                direction = new Vector2(player.direction, 0f);
            direction = direction.SafeNormalize(Vector2.Zero);

            player.velocity = direction * DashSpeed;
            player.immune = true;
            player.immuneTime = InvincibleFrames;
            player.immuneNoBlink = false;

            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, DustID.BlueFlare, 0f, 0f);
                dust.velocity = -direction * Main.rand.NextFloat(2f, 5f) + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.8f);
                dust.color = new Color(187, 91, 201);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "QiDunEffect", "化为气遁，向鼠标方向高速冲刺"));
            tooltips.Add(new TooltipLine(Mod, "QiDunInvincible", $"冲刺期间无敌{InvincibleFrames}帧"));
            tooltips.Add(new TooltipLine(Mod, "QiDunQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
