using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转变化道辅助蛊", "二转", "变")]
    public class DingXingGu : ModItem
    {
        private const int QiCostToUse = 10;

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 3000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useAnimation = 30;
            Item.useTime = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = false;
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

            player.AddBuff(ModContent.BuffType<DingXingBuff>(), 600);

            return true;
        }
    }
}
