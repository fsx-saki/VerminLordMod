using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 药堂家老 — 古月家族药堂负责人，三转治疗蛊师
    /// 重写自原有 YaoTangJiaLao，继承 GuYueNPCBase
    /// </summary>
    [AutoloadHead]
    public class GuYueMedicineElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.MedicineElder;
        public new const string ShopName = "MedicineElderShop";

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
                return "药堂家老温和地说：\"受伤了就来药堂，我给你看看。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "药堂家老关切地说：\"修行路上，保重身体要紧。\"";
            else
                return "药堂家老微笑着递给你一株草药：\"拿着，对你有好处。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName)
                .Add(new Item(ItemID.HealingPotion)
                {
                    shopCustomPrice = 10,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                })
                .Add(new Item(ModContent.ItemType<Items.Consumables.LivingLeaf>())
                {
                    shopCustomPrice = 50,
                    shopSpecialCurrency = VerminLordMod.YuanSId
                });
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 15;
            knockback = 3f;
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
            multiplier = 10f;
            randomOffset = 0f;
        }
    }
}
