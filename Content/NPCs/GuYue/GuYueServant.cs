using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 杂役 — 古月家族的杂役，负责日常杂务
    /// </summary>
    [AutoloadHead]
    public class GuYueServant : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.Servant;
        public new const string ShopName = "ServantShop";

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            foreach (var player in Main.ActivePlayers)
            {
                var qiRealm = player.GetModPlayer<Common.Players.QiRealmPlayer>();
                if (qiRealm.GuLevel > 0)
                    return true;
            }
            return false;
        }

        protected override string GetFriendlyDialogue()
        {
            if (NumberOfTimesTalkedTo == 1)
                return "杂役擦了擦汗：\"您有什么吩咐？我正忙着打扫呢。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "杂役憨厚地笑了笑：\"虽然我只是个杂役，但能为家族出力就很高兴了。\"";
            else
                return "杂役点头哈腰：\"您慢走，有什么需要尽管吩咐。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 6;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 10;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = 0; // 近战攻击 - 使用铜斧/铜镐
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 0f;
            randomOffset = 0f;
        }

        public override void OnKill()
        {
            base.OnKill();
            if (Main.netMode != NetmodeID.Server)
            {
                // 杂役掉落：铜矿、铜币、铜工具
                int itemChoice = Main.rand.Next(4);
                switch (itemChoice)
                {
                    case 0:
                        Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.CopperAxe);
                        break;
                    case 1:
                        Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.CopperPickaxe);
                        break;
                    case 2:
                        Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.CopperBar, Main.rand.Next(1, 4));
                        break;
                    case 3:
                        Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.CopperOre, Main.rand.Next(3, 8));
                        break;
                }
                // 额外掉落铜币
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.CopperCoin, Main.rand.Next(10, 30));
            }
        }
    }
}
