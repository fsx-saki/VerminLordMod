using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;

namespace VerminLordMod.Content.Items.QuestItems
{
    public class YuanHaiBox : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.maxStack = 1;
            Item.value = 0;
            Item.rare = ItemRarityID.LightPurple;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.autoReuse = false;
            Item.consumable = true;
        }

        public override string Texture => "Terraria/Images/Item_" + ItemID.IronCrate;

        public override bool CanUseItem(Player player) => !player.GetModPlayer<YuanHaiFlags>().Activated;

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer) return null;
            var flags = player.GetModPlayer<YuanHaiFlags>();
            if (flags.Activated) return false;

            var talent = player.GetModPlayer<QiTalentPlayer>();
            talent.OnAwakening(QiTalentPlayer.TalentGrade.Jia);
            int maxQi = (int)(1 * 100 * talent.GetZiZhiMultiplier() / 10f);

            flags.Activated = true; flags.IsJiaGrade = true;
            flags.QiCurrent = maxQi; flags.QiMax = maxQi;
            Main.NewText($"[元海] 元海已激活。资质：甲等。真元上限：{maxQi}", new Microsoft.Xna.Framework.Color(180, 120, 255));

            for (int i = 0; i < player.inventory.Length; i++)
                if (!player.inventory[i].IsAir && player.inventory[i].type == ModContent.ItemType<YuanHaiBox>())
                { player.inventory[i].TurnToAir(); break; }
            player.QuickSpawnItem(player.GetSource_GiftOrReward(), ModContent.ItemType<YuanHaiScrap>(), 1);
            return true;
        }

        public override void ModifyTooltips(System.Collections.Generic.List<TooltipLine> tooltips)
        {
            TooltipLine nameLine = tooltips.Find(l => l.Name == "ItemName");
            if (nameLine != null) nameLine.Text = "一团金属制成的歪歪扭扭的盒子";
            tooltips.Add(new TooltipLine(Mod, "Desc", "系统拾取你生活垃圾中的金属随手捏的玩意"));
            tooltips.Add(new TooltipLine(Mod, "Effect", "使用后激活元海（甲等资质）"));
        }
    }
}
