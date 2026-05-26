using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using Terraria.GameContent;
using Microsoft.Xna.Framework;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "一转运道功能蛊", "一转", "运")]
    public class YiWaiGu : ModItem
    {
        private const int QiCostPerUse = 5;

        private static readonly int[] CoinTypes = new int[]
        {
            ItemID.CopperCoin,
            ItemID.SilverCoin,
            ItemID.GoldCoin,
            ItemID.PlatinumCoin
        };

        private static readonly int[] RandomBuffTypes = new int[]
        {
            BuffID.Regeneration,
            BuffID.Swiftness,
            BuffID.Ironskin,
            BuffID.ManaRegeneration,
            BuffID.Archery,
            BuffID.Sharpened,
            BuffID.WellFed,
            BuffID.Heartreach
        };

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.White;
            Item.maxStack = 20;
            Item.value = 500;
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
            return qiResource.QiCurrent >= QiCostPerUse;
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return null;

            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostPerUse);

            int roll = Main.rand.Next(1, 6);

            switch (roll)
            {
                case 1:
                    player.Heal(50);
                    CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(50, 255, 50), "意外之喜！");
                    break;

                case 2:
                    player.immune = true;
                    player.immuneTime = 30;
                    player.SetImmuneTimeForAllTypes(30);
                    CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(100, 200, 255), "幸运闪避！");
                    break;

                case 3:
                    for (int i = 0; i < 3; i++)
                    {
                        int coinType = CoinTypes[Main.rand.Next(CoinTypes.Length)];
                        int stack = coinType == ItemID.PlatinumCoin ? 1 : Main.rand.Next(1, 5);
                        player.QuickSpawnItem(null, coinType, stack);
                    }
                    CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(255, 215, 0), "意外之财！");
                    break;

                case 4:
                    player.Hurt(PlayerDeathReason.LegacyDefault(), 20, 0);
                    CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(255, 50, 50), "意外之灾！");
                    break;

                case 5:
                    int buffType = RandomBuffTypes[Main.rand.Next(RandomBuffTypes.Length)];
                    player.AddBuff(buffType, 300);
                    CombatText.NewText(player.Hitbox, new Microsoft.Xna.Framework.Color(200, 150, 255), "意外之缘！");
                    break;
            }

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.Add(new TooltipLine(Mod, "YiWaiEffect", "意外：使用后随机触发一种效果"));
            tooltips.Add(new TooltipLine(Mod, "YiWaiQiCost", $"消耗真元：{QiCostPerUse}"));
        }
    }
}
