using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转盗道功能蛊屋", "二转", "盗")]
    public class 贼巢 : ModItem
    {
        private const int QiCostPerUse = 12;
        private const int BuffDuration = 300;

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
            Item.UseSound = SoundID.Item3;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
            {
                Text.ShowTextRed(player, "真元不足，无法催动贼巢");
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

            player.AddBuff(BuffID.ShadowDodge, BuffDuration);
            player.GetModPlayer<ZeiChaoPlayer>().StealthTimer = BuffDuration;

            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height,
                    DustID.Shadowflame, 0f, 0f);
                dust.velocity = Main.rand.NextVector2Circular(2f, 2f);
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }

            Text.ShowTextGreen(player, "贼巢：隐匿于暗影之中！");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "ZeiChaoDesc", "二转盗道功能蛊屋：贼巢"));
            tooltips.Add(new TooltipLine(Mod, "ZeiChaoEffect", "隐身5秒，隐身期间+20%移动速度"));
            tooltips.Add(new TooltipLine(Mod, "ZeiChaoQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }

    public class ZeiChaoPlayer : ModPlayer
    {
        public int StealthTimer { get; set; }

        public override void ResetEffects()
        {
            if (StealthTimer > 0)
                StealthTimer--;
        }

        public override void PostUpdateEquips()
        {
            if (StealthTimer > 0)
            {
                Player.moveSpeed += 0.20f;

                if (Main.rand.NextBool(8))
                {
                    var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Shadowflame);
                    d.velocity *= 0.15f;
                    d.noGravity = true;
                    d.scale = Main.rand.NextFloat(0.5f, 0.8f);
                    d.color = new Color(80, 0, 120);
                }
            }
        }

        public override void UpdateDead()
        {
            StealthTimer = 0;
        }
    }
}
