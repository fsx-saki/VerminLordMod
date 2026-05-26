using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YiXinSanYongBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<YiXinSanYongPlayer>().HasYiXinSanYong = true;
            player.GetCritChance(DamageClass.Generic) += 5f;
            player.moveSpeed += 0.10f;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.AncientLight);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class YiXinSanYongPlayer : ModPlayer
    {
        public bool HasYiXinSanYong { get; set; }

        public override void ResetEffects()
        {
            HasYiXinSanYong = false;
        }

        public override float UseSpeedMultiplier(Item item)
        {
            if (HasYiXinSanYong)
            {
                return 1.15f;
            }
            return 1f;
        }
    }
}
