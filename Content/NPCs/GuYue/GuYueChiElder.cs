using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 赤脉家老 — 古月家族赤脉当权家老，性格高傲
    /// </summary>
    [AutoloadHead]
    public class GuYueChiElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.ChiElder;
        public new const string ShopName = "ChiElderShop";

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
                return "赤脉家老傲然道：\"我赤脉一系，向来是家族的中流砥柱。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "赤脉家老冷哼一声：\"好好修行，别给家族丢脸。\"";
            else
                return "赤脉家老斜睨了你一眼：\"嗯，还算有点出息。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 25;
            knockback = 5f;
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
