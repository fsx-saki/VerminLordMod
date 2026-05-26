using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转风道功能蛊", "二转", "风")]
    public class XuanZhongGu : ModItem
    {
        private const int QiCostPerUse = 12;
        private const int DodgeSpeed = 15;
        private const int InvincibilityFrames = 30;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = false;
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

            Vector2 direction = player.Center - Main.MouseWorld;
            if (direction == Vector2.Zero)
                direction = new Vector2(0, -1);
            direction = direction.SafeNormalize(Vector2.Zero);
            player.velocity = direction * DodgeSpeed;

            player.immune = true;
            player.immuneTime = InvincibilityFrames;
            player.AddBuff(BuffID.ShadowDodge, InvincibilityFrames);

            for (int i = 0; i < 10; i++)
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Cloud);
                d.velocity = -direction * Main.rand.NextFloat(1f, 3f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "XuanZhongEffect", "旋踵：向鼠标反方向快速闪避，获得短暂无敌（转身即走）"));
            tooltips.Add(new TooltipLine(Mod, "XuanZhongQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
