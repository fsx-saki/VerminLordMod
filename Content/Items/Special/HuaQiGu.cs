using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转气道辅助蛊", "二转", "气")]
    public class HuaQiGu : ModItem
    {
        private const float HpConvertRatio = 0.3f;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 5000;
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
            return player.statLife > 1;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            int currentHp = player.statLife;
            int hpToConvert = (int)(currentHp * HpConvertRatio);

            if (hpToConvert <= 0)
                return false;

            player.statLife -= hpToConvert;
            if (player.statLife < 1)
                player.statLife = 1;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.RefundQi(hpToConvert);

            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(player.position, player.width, player.height, 261, 0f, 0f);
                dust.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-3f, -1f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(1f, 1.5f);
            }

            player.HealEffect(0, false);

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "HuaQiEffect", $"将当前生命值的30%转化为真元"));
            tooltips.Add(new TooltipLine(Mod, "HuaQiNote", "此蛊产生真元，不消耗真元"));
        }
    }
}
