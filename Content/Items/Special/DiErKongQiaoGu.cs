using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转炼道功能蛊", "二转", "炼")]
    public class DiErKongQiaoGu : ModItem
    {
        private const int QiCostToUse = 15;

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 10;
            Item.value = 5000;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostToUse;
        }

        public override bool? UseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            if (!qiResource.ConsumeQi(QiCostToUse))
                return false;

            var diErKongQiao = player.GetModPlayer<DiErKongQiaoPlayer>();
            diErKongQiao.QiRegenBonus += 0.05f;

            return true;
        }
    }

    public class DiErKongQiaoPlayer : ModPlayer
    {
        public float QiRegenBonus { get; set; }

        public override void ResetEffects()
        {
        }

        public override void PostUpdateEquips()
        {
            Player.GetModPlayer<QiResourcePlayer>().ExtraQiRegen += QiRegenBonus;
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            tag["QiRegenBonus"] = QiRegenBonus;
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            QiRegenBonus = tag.GetFloat("QiRegenBonus");
        }
    }
}
