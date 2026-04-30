using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 学堂家老 — 古月家族学堂负责人，三转蛊师
    /// 重写自原有 XueTangJiaLao，继承 GuYueNPCBase
    /// </summary>
    [AutoloadHead]
    public class GuYueSchoolElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.SchoolElder;
        public new const string ShopName = "SchoolElderShop";

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
                return "学堂家老抚须微笑：\"年轻人，可愿听老夫讲讲蛊师的奥妙？\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "学堂家老耐心地说：\"蛊师一道，贵在坚持。\"";
            else
                return "学堂家老点点头：\"不错，你有空可以多来学堂坐坐。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName)
                .Add(new Item(ModContent.ItemType<Items.Consumables.WanShi>())
                {
                    shopCustomPrice = 5,
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
            cooldown = 30;
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
