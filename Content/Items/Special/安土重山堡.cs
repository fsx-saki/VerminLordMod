using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Items.Accessories;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "四转泥道防御蛊屋", "四转", "泥")]
    public class 安土重山堡 : GuBaseItem
    {
        protected override int _guLevel => 4;
        protected override int qiCost => 25;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.LightPurple;
            Item.maxStack = 1;
            Item.value = 100000;
            Item.accessory = true;
        }

        public override void OnActiveTick(Player player)
        {
            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (_guLevel > qiRealm.GuLevel && Main.rand.NextBool(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(Terraria.DataStructures.PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }
            qiResource.QiMaxCurrent -= qiCost;

            player.statDefense += 15;
            player.endurance += 0.10f;

            if (player.velocity.Y == 0f)
            {
                player.statDefense += 5;
                player.noKnockback = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "AnTuEffect1", "增加15点防御力，减少10%受到的伤害"));
            tooltips.Add(new TooltipLine(Mod, "AnTuEffect2", "站在地面时额外增加5点防御力且免疫击退"));
            tooltips.Add(new TooltipLine(Mod, "AnTuQiCost", $"占据真元：{qiCost}"));
        }
    }
}
