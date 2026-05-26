using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.ImplementationTracker;
using VerminLordMod.Common.Players;
using VerminLordMod.Content.Buffs.AddToSelf.Pobuff;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Content.Items.Special
{
    [ImplStatus(ImplStatus.Implemented, "二转智道辅助蛊", "二转", "智")]
    public class BuXianGu : ModItem
    {
        private const int QiCostValue = 12;

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.rare = ItemRarityID.Green;
            Item.maxStack = 1;
            Item.value = 500000;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.UseSound = SoundID.Item4;
            Item.consumable = false;
            Item.autoReuse = false;
        }

        public override bool CanUseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            return qiResource.QiCurrent >= QiCostValue;
        }

        public override bool? UseItem(Player player)
        {
            var qiResource = player.GetModPlayer<QiResourcePlayer>();
            qiResource.ConsumeQi(QiCostValue);

            RevealUpcomingEnemies(player);

            player.AddBuff(ModContent.BuffType<BuXianBuff>(), 480);

            return true;
        }

        private void RevealUpcomingEnemies(Player player)
        {
            int revealed = 0;
            for (int i = 0; i < Main.maxNPCs && revealed < 3; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    CombatText.NewText(
                        new Microsoft.Xna.Framework.Rectangle(
                            (int)player.Center.X - 40,
                            (int)player.Center.Y - 60 - revealed * 25,
                            80, 20),
                        new Microsoft.Xna.Framework.Color(100, 200, 255),
                        npc.TypeName,
                        true);
                    revealed++;
                }
            }

            if (revealed == 0)
            {
                CombatText.NewText(
                    player.Hitbox,
                    new Microsoft.Xna.Framework.Color(100, 200, 255),
                    "卜卦: 周围无敌",
                    true);
            }
        }
    }
}
