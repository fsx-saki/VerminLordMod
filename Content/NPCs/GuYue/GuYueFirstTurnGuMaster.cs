using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 古月一转蛊师 — 普通家族蛊师学员/成员
    /// </summary>
    [AutoloadHead]
    public class GuYueFirstTurnGuMaster : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.FirstTurnGuMaster;
        public new const string ShopName = "FirstTurnGuMasterShop";

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
                return "古月蛊师好奇地看着你：\"你是新来的？我也是刚成为蛊师不久。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "古月蛊师兴奋地说：\"我最近又学会了一个新蛊术！\"";
            else
                return "古月蛊师笑道：\"一起努力修行吧！\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 10;
            knockback = 2f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 10;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<Projectiles.MoonlightProj>();
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 8f;
            randomOffset = 0f;
        }
    }
}
