using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "人道特殊蛊-功德积累", "不定", "人")]
    public class JiDeGu : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Lime;
            Item.maxStack = 1;
            Item.value = 500000;
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
            return player.whoAmI == Main.myPlayer;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var jiDePlayer = player.GetModPlayer<JiDePlayer>();

            if (player.altFunctionUse == 2)
            {
                if (jiDePlayer.Virtue <= 0)
                {
                    Text.ShowTextRed(player, "功德不足，无法消耗积德蛊");
                    return false;
                }

                int qiBonus = jiDePlayer.Virtue * 10;
                float regenBonus = jiDePlayer.Virtue * 0.1f;
                jiDePlayer.QiMaxBonus += qiBonus;
                jiDePlayer.QiRegenBonus += regenBonus;
                jiDePlayer.Virtue = 0;

                Text.ShowMessageGold($"积德蛊消耗！永久增加真元上限{qiBonus}，恢复速率+{regenBonus:F1}");

                Item.TurnToAir();
                return true;
            }

            Text.ShowMessageGold($"当前功德值：{jiDePlayer.Virtue}");
            return true;
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override void UpdateInventory(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return;

            if (player.HeldItem.type == Item.type)
            {
                var jiDePlayer = player.GetModPlayer<JiDePlayer>();
                jiDePlayer.HasJiDeGuInHand = true;
            }
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var jiDePlayer = Main.LocalPlayer.GetModPlayer<JiDePlayer>();
            tooltips.Add(new TooltipLine(Mod, "JiDeVirtue", $"当前功德值：{jiDePlayer.Virtue}"));
            tooltips.Add(new TooltipLine(Mod, "JiDeLeftClick", "左键：查看当前功德值"));
            tooltips.Add(new TooltipLine(Mod, "JiDeRightClick", "右键：消耗此蛊，永久提升真元"));
            tooltips.Add(new TooltipLine(Mod, "JiDeEffect", $"持有击杀NPC可积累功德"));
            tooltips.Add(new TooltipLine(Mod, "JiDeConsume", $"消耗效果：真元上限+功德×10，恢复速率+功德×0.1"));
        }
    }
}
