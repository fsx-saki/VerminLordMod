using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.YuanHaiSystem;

namespace VerminLordMod.Content.Items.QuestItems
{
    public class YuanShi : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24; Item.height = 24;
            Item.maxStack = 9999;
            Item.rare = ItemRarityID.LightPurple;
            Item.value = 100;
            Item.consumable = true;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.UseSound = SoundID.Item4;
        }

        public override string Texture => "Terraria/Images/Item_" + ItemID.Amethyst;

        public override bool CanUseItem(Player player)
        {
            // 炼化界面打开时禁用左键使用，防止和 UI 槽交互冲突
            var refining = RefiningUI.Instance;
            return refining == null || !refining.Visible;
        }

        public override bool? UseItem(Player player)
        {
            var qi = player.GetModPlayer<QiResourcePlayer>();
            if (qi == null) return true;
            qi.QiCurrent = Microsoft.Xna.Framework.MathHelper.Min(qi.QiMaxCurrent, qi.QiCurrent + 16);
            return true;
        }
    }
}
