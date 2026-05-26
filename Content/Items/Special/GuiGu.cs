using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "概念级规则道-捕蛊", "概念级", "规则")]
    public class GuiGu : ModItem
    {
        private int _cooldownTimer = 0;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Red;
            Item.maxStack = 1;
            Item.value = 10000000;
            Item.consumable = false;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return false;

            if (_cooldownTimer > 0)
            {
                Text.ShowTextRed(player, $"规蛊冷却中...{(_cooldownTimer / 60) + 1}秒");
                return false;
            }

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < 200)
            {
                Text.ShowTextRed(player, "真元不足，无法驱动规蛊");
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(200);

            int buffType = ModContent.BuffType<GuiGuBuff>();
            player.AddBuff(buffType, 600);

            _cooldownTimer = 3600;

            Text.ShowMessageGold("规矩捕蛊！天地为网，万蛊来朝！");
            Text.ShowTextGreen(player, "规蛊激活：击杀敌人50%概率额外掉落蛊虫");

            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (_cooldownTimer > 0)
                _cooldownTimer--;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "GuiGuDesc", "概念级规则道 — 与矩蛊并称天地双绝"));
            tooltips.Add(new TooltipLine(Mod, "GuiGuEffect", "激活后600帧内，击杀敌人50%概率额外掉落蛊虫"));
            tooltips.Add(new TooltipLine(Mod, "GuiGuCost", "消耗真元：200"));
            tooltips.Add(new TooltipLine(Mod, "GuiGuCooldown", "冷却时间：60秒"));

            if (_cooldownTimer > 0)
                tooltips.Add(new TooltipLine(Mod, "GuiGuTimer", $"当前冷却：{(_cooldownTimer / 60) + 1}秒"));
        }
    }
}
