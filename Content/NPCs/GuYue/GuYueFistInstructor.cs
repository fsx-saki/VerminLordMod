using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 拳脚教头 — 古月家族拳脚教头，负责教导族人拳脚功夫
    /// </summary>
    [AutoloadHead]
    public class GuYueFistInstructor : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.FistInstructor;
        public new const string ShopName = "FistInstructorShop";

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
                return "拳脚教头拍了拍胸脯：\"想学拳脚功夫？找我准没错！\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "拳脚教头挥舞着拳头：\"拳脚是蛊师的基础，根基不牢，地动山摇！\"";
            else
                return "拳脚教头大笑道：\"来，陪我练练！\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName);
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 18;
            knockback = 6f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 15;
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
