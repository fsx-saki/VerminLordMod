using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 御堂家老 — 古月家族御堂负责人，三转蛊师
    /// 重写自原有 YuTangJiaLao，继承 GuYueNPCBase
    /// </summary>
    [AutoloadHead]
    public class GuYueDefenseElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.DefenseElder;
        public new const string ShopName = "DefenseElderShop";

        public override bool CanTownNPCSpawn(int numTownNPCs)
        {
            foreach (var player in Main.ActivePlayers)
            {
                var qiRealm = player.GetModPlayer<Common.Players.QiRealmPlayer>();
                if (qiRealm.GuLevel > 0 && player.statLifeMax2 > 120)
                    return true;
            }
            return false;
        }

        protected override string GetFriendlyDialogue()
        {
            if (NumberOfTimesTalkedTo == 1)
                return "御堂家老打量着你：\"想挑选些防身之物？我这里倒是有几件好货。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "御堂家老拍了拍身上的铠甲：\"防御之道，在于未雨绸缪。\"";
            else
                return "御堂家老点点头：\"有需要就来御堂看看。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName)
                .Add(new Item(ModContent.ItemType<Items.Accessories.One.JadeSkin>())
                {
                    shopCustomPrice = 250,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                })
                .Add(new Item(ModContent.ItemType<Items.Accessories.One.StoneSkin>())
                {
                    shopCustomPrice = 50,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                })
                .Add(new Item(ModContent.ItemType<Items.Accessories.One.IronSkin>())
                {
                    shopCustomPrice = 120,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                })
                .Add(new Item(ModContent.ItemType<Items.Accessories.One.CopperSkin>())
                {
                    shopCustomPrice = 100,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                });
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 20;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 10;
            randExtraCooldown = 0;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<Projectiles.MoonlightProj>();
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 12f;
            randomOffset = 0f;
        }
    }
}
