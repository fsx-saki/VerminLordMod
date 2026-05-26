using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "五转炼道仙蛊", "五转", "炼")]
    public class TianYuanBaoHuangLianXianGu : ModItem
    {
        private const int QiCostPerUse = 45;
        private const int CooldownTime = 3600;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 1;
            Item.value = 1000000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;

            var tianYuanPlayer = player.GetModPlayer<TianYuanPlayer>();
            if (tianYuanPlayer.IsOnCooldown)
                return false;

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int buffType = ModContent.BuffType<TianYuanBuff>();
            player.AddBuff(buffType, 300);

            var tianYuanPlayer = player.GetModPlayer<TianYuanPlayer>();
            tianYuanPlayer.CooldownTimer = CooldownTime;

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TianYuanEffect", "天元宝皇炼仙！全属性+15%，持续5秒"));
            tooltips.Add(new TooltipLine(Mod, "TianYuanExhaust", "效果结束后虚弱5秒（炼化反噬）"));
            tooltips.Add(new TooltipLine(Mod, "TianYuanCooldown", "冷却时间：60秒"));
            tooltips.Add(new TooltipLine(Mod, "TianYuanQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
