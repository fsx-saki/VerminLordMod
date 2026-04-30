using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.NPCs.GuYue
{
    /// <summary>
    /// 古月族长 — 古月博，四转蛊师，家族最高领袖
    /// </summary>
    [AutoloadHead]
    public class GuYueChief : GuYueNPCBase
    {
        public override GuYueNPCType GetNPCType() => GuYueNPCType.Chief;
        public new const string ShopName = "ChiefShop";

        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
        }

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
                return "古月族长威严地看着你：\"欢迎来到古月山寨，年轻人。\"";
            else if (NumberOfTimesTalkedTo <= 3)
                return "古月族长微微点头：\"在寨子里好好修行，将来为家族出力。\"";
            else
                return "古月族长拍了拍你的肩膀：\"好好干，我看好你。\"";
        }

        public override void AddShops()
        {
            var shop = new NPCShop(Type, ShopName)
                .Add(new Item(ItemID.Mushroom));
            shop.Register();
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 30;
            knockback = 6f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 20;
            randExtraCooldown = 0;
        }

        public override void TownNPCAttackProj(ref int projType, ref int attackDelay)
        {
            projType = ModContent.ProjectileType<Projectiles.MoonlightProj>();
            attackDelay = 1;
        }

        public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset)
        {
            multiplier = 14f;
            randomOffset = 0f;
        }
    }
}
