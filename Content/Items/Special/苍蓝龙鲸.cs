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
    [ImplStatus(ImplStatus.Implemented, "五转水道防御仙蛊", "五转", "水")]
    class 苍蓝龙鲸 : GuBaseItem
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

            player.gills = true;
            player.moveSpeed += 0.2f;

            if (player.wet || player.lavaWet || player.honeyWet)
            {
                player.statDefense += 5;
                player.lifeRegen += 4;
            }

            var qiRealm = player.GetModPlayer<QiRealmPlayer>();
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (_guLevel > qiRealm.GuLevel && Randommer.Roll(10))
            {
                Text.ShowTextRed(player, "您正在强行调动高转蛊虫！！！");
                player.Hurt(Terraria.DataStructures.PlayerDeathReason.LegacyDefault(), (_guLevel - qiRealm.GuLevel) * Main.LocalPlayer.statLifeMax2 / 20, 0);
            }
            qiResource.QiMaxCurrent -= qiCost;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "CangLanLongJingDef", "增加12点防御力，可在水中呼吸"));
            tooltips.Add(new TooltipLine(Mod, "CangLanLongJingWater", "水中移动速度+20%"));
            tooltips.Add(new TooltipLine(Mod, "CangLanLongJingWaterBonus", "在水中额外+5防御，每秒回复2点生命"));
            tooltips.Add(new TooltipLine(Mod, "CangLanLongJingQiCost", $"占据真元：{qiCost}"));
        }
    }
}
