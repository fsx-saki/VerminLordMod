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
    [ImplStatus(ImplStatus.Implemented, "五转泥道防御蛊", "五转", "泥")]
    public class HuoXiNiXianGu : GuBaseItem
    {
        protected override int _guLevel => 5;
        protected override int qiCost => 25;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.accessory = true;
            Item.defense = 18;
        }

        public override void OnActiveTick(Player player)
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            player.endurance += 0.10f;

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }
            qiResource.QiMaxCurrent -= qiCost;

            player.GetModPlayer<HuoXiNiPlayer>().HuoXiNiActive = true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HuoXiNiEffect", "增加18点防御力，减少10%受到的伤害"));
            tooltips.Add(new TooltipLine(Mod, "HuoXiNiShield", "受到超过40点伤害时，生成泥盾完全吸收下一次攻击（冷却15秒）"));
            tooltips.Add(new TooltipLine(Mod, "HuoXiNiQiCost", $"占据真元：{qiCost}"));
        }
    }
}
