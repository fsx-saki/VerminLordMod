using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转泥道防御蛊", "二转", "泥")]
    public class DiQiaoWoNiu : GuBaseItem
    {
        protected override int _guLevel => 2;
        protected override int qiCost => 10;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
            Item.accessory = true;
            Item.defense = 6;
        }

        public override void OnActiveTick(Player player)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            player.moveSpeed -= 0.08f;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }
            qiResource.QiMaxCurrent -= qiCost;

            player.GetModPlayer<DiQiaoWoNiuPlayer>().ReflectActive = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "DiQiaoEffect", "增加6点防御力，减少8%移动速度，15%概率反弹弹幕"));
            tooltips.Add(new TooltipLine(Mod, "DiQiaoQiCost", $"占据真元：{qiCost}"));
        }
    }

    public class DiQiaoWoNiuPlayer : ModPlayer
    {
        public bool ReflectActive { get; set; }
        private const float ReflectChance = 0.15f;

        public override void ResetEffects()
        {
            ReflectActive = false;
        }

        public override bool CanBeHitByProjectile(Projectile proj)
        {
            if (!ReflectActive)
                return true;

            if (proj.friendly && !proj.hostile)
                return true;

            if (Main.rand.NextFloat() < ReflectChance)
            {
                if (Player.whoAmI == Main.myPlayer)
                {
                    proj.hostile = false;
                    proj.friendly = true;
                    proj.velocity *= -1f;
                    proj.owner = Player.whoAmI;
                    proj.damage = (int)(proj.damage * 0.5f);
                    Text.ShowTextGreen(Player, "地壳蜗牛反弹了弹幕！");
                }
                return false;
            }

            return true;
        }
    }
}
