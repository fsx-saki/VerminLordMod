using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转天道辅助蛊", "一转", "天")]
    public class TianShengGu : ModItem
    {
        private const int QiCostPerUse = 8;
        private const int QiMaxBonus = 10;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Cyan;
            Item.maxStack = 10;
            Item.value = 1000000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item46;
            Item.autoReuse = false;
            Item.useTurn = true;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            var tianShengPlayer = player.GetModPlayer<TianShengPlayer>();
            if (qiResource.QiCurrent < QiCostPerUse)
                return false;
            if (tianShengPlayer.BonusQiMax >= QiMaxBonus * Item.maxStack)
                return false;
            return true;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            var tianShengPlayer = player.GetModPlayer<TianShengPlayer>();
            tianShengPlayer.BonusQiMax += QiMaxBonus;

            Text.ShowTextGreen(player, $"天生蛊觉醒！真元上限永久+{QiMaxBonus}");

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "TianShengEffect", "天生资质觉醒，真元上限永久+10"));
            tooltips.Add(new TooltipLine(Mod, "TianShengStack", $"最多使用{Item.maxStack}次"));
            tooltips.Add(new TooltipLine(Mod, "TianShengQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }

    public class TianShengPlayer : ModPlayer
    {
        public int BonusQiMax { get; set; }

        public override void PostUpdateEquips()
        {
            var qiResource = Player.GetModPlayer<QiResourcePlayer>();
            qiResource.QiMaxBase += BonusQiMax;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["BonusQiMax"] = BonusQiMax;
        }

        public override void LoadData(TagCompound tag)
        {
            BonusQiMax = tag.GetInt("BonusQiMax");
        }
    }
}
