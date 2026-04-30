using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 漠脉家老 — 古月家族漠脉当权家老，老成持重
    /// </summary>
    [AutoloadHead]
    public class GuYueMoElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.MoElder;
        public new const string ShopName = "MoElderShop";

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
                return "漠脉家老沉稳地说：\"年轻人，稳扎稳打才是修行正道。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "漠脉家老语重心长：\"修行如逆水行舟，不进则退。\"";
            else
                return "漠脉家老微微颔首：\"不错，继续保持。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 22;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 25;
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
