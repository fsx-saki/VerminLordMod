using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 药脉家老 — 古月家族药脉家主（古月药姬），精通治疗
    /// </summary>
    [AutoloadHead]
    public class GuYueMedicinePulseElder : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.MedicinePulseElder;
        public new const string ShopName = "MedicinePulseElderShop";

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
                return "药脉家老温柔地说：\"受伤了就来药堂，我会尽力医治。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "药脉家老关切道：\"你的气色不太好，要注意休息。\"";
            else
                return "药脉家老微笑着递给你一瓶药散：\"这是我调制的伤药，拿着备用。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 12;
            knockback = 3f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 15;
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
