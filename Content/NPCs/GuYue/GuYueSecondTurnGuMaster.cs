using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 古月二转蛊师 — 资深家族蛊师，已具备一定实力
    /// </summary>
    [AutoloadHead]
    public class GuYueSecondTurnGuMaster : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.SecondTurnGuMaster;
        public new const string ShopName = "SecondTurnGuMasterShop";

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
                return "古月资深蛊师打量着你：\"二转修为？不错，有前途。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "古月资深蛊师感慨道：\"修行之路漫长，我花了五年才到二转。\"";
            else
                return "古月资深蛊师拍拍你的肩：\"有什么不懂的可以问我。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 15;
            knockback = 3f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 25;
            randExtraCooldown = 5;
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
