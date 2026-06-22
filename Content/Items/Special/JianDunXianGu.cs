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
    [ImplStatus(ImplStatus.Implemented, "五转战道防御仙蛊", "五转", "战")]
    public class JianDunXianGu : GuBaseItem
    {
        protected override int _guLevel => 5;
        protected override int qiCost => 36;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightRed;
            Item.maxStack = 1;
            Item.value = 50000;
            Item.accessory = true;
            Item.defense = 19;
        }

        public override void OnActiveTick(Player player)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            player.GetDamage(DamageClass.Generic) += 0.08f;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }
            qiResource.QiMaxCurrent -= qiCost;

            player.GetModPlayer<JianDunXianPlayer>().JianDunXianActive = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JianDunXianEffect", "增加12点防御力，增加8%伤害"));
            tooltips.Add(new TooltipLine(Mod, "JianDunXianParry", "15%概率完全格挡伤害（剑盾格挡）"));
            tooltips.Add(new TooltipLine(Mod, "JianDunXianQiCost", $"占据真元：{qiCost}"));
        }
    }
}
