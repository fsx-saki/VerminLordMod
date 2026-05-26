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
    [ImplStatus(ImplStatus.Implemented, "概念级规则道-防御", "概念级", "规则")]
    public class JuGu : ModItem
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
                Text.ShowTextRed(player, $"矩蛊冷却中...{(_cooldownTimer / 60) + 1}秒");
                return false;
            }

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (qiResource.QiCurrent < 200)
            {
                Text.ShowTextRed(player, "真元不足，无法驱动矩蛊");
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

            int shieldBuff = ModContent.BuffType<JuGuShieldBuff>();
            player.AddBuff(shieldBuff, 300);

            _cooldownTimer = 3600;

            Text.ShowMessageGold("矩蛊！方圆之间，万法不侵！");
            Text.ShowTextGreen(player, "矩蛊护盾激活：5秒内完全无敌");

            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (_cooldownTimer > 0)
                _cooldownTimer--;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "JuGuDesc", "概念级规则道 — 与规蛊并称天地双绝"));
            tooltips.Add(new TooltipLine(Mod, "JuGuEffect", "激活后300帧内完全无敌"));
            tooltips.Add(new TooltipLine(Mod, "JuGuExhaust", "无敌结束后：10秒内伤害和移速降低50%"));
            tooltips.Add(new TooltipLine(Mod, "JuGuCost", "消耗真元：200"));
            tooltips.Add(new TooltipLine(Mod, "JuGuCooldown", "冷却时间：60秒"));

            if (_cooldownTimer > 0)
                tooltips.Add(new TooltipLine(Mod, "JuGuTimer", $"当前冷却：{(_cooldownTimer / 60) + 1}秒"));
        }
    }
}
