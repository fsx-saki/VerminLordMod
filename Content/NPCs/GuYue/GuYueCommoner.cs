using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 古月凡人 — 古月家族的凡人成员，尚未开辟空窍
    /// </summary>
    [AutoloadHead]
    public class GuYueCommoner : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.Commoner;
        public new const string ShopName = "CommonerShop";

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
                return "古月凡人羡慕地看着你：\"您是蛊师大人吧？真厉害！\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "古月凡人叹了口气：\"我资质太差，怕是这辈子都开不了空窍了。\"";
            else
                return "古月凡人笑道：\"能在古月山寨生活，已经很知足了。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 4;
            knockback = 3f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 40;
            randExtraCooldown = 15;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = 0; // 近战攻击 - 使用木剑
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
                // 凡人掉落：木剑、木材、铜币
                int itemChoice = Main.rand.Next(3);
                switch (itemChoice)
                {
                    case 0:
                        Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.WoodenSword);
                        break;
                    case 1:
                        Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.Wood, Main.rand.Next(5, 15));
                        break;
                    case 2:
                        Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.Gel, Main.rand.Next(1, 3));
                        break;
                }
                // 额外掉落铜币
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ItemID.CopperCoin, Main.rand.Next(5, 15));
            }
        }
    }
}
