using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YongShengBuff : ModBuff
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
            if (player.statLife < 1)
                player.statLife = 1;

            player.immune = true;
            player.immuneTime = 2;

            if (Main.rand.NextBool(3))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.CrystalPulse2);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.5f);
                d.velocity *= 0.3f;
                d.velocity.Y -= 1f;
            }
        }
    }

    public class YongShengExhaustBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
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
            player.GetDamage(DamageClass.Generic) -= 0.5f;
            player.moveSpeed *= 0.5f;
            player.maxRunSpeed *= 0.5f;
            player.runAcceleration *= 0.5f;
            player.GetDamage(DamageClass.Magic) -= 0.5f;
            player.GetDamage(DamageClass.Melee) -= 0.5f;
            player.GetDamage(DamageClass.Ranged) -= 0.5f;
            player.GetDamage(DamageClass.Summon) -= 0.5f;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.1f;
                d.noGravity = true;
                d.scale = 0.6f;
                d.alpha = 150;
            }
        }
    }

    public class YongShengCDBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.debuff[Type] = true;
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
            if (player.buffTime[buffIndex] == 1)
            {
                player.AddBuff(ModContent.BuffType<YongShengExhaustBuff>(), 600);
            }
        }
    }
}
