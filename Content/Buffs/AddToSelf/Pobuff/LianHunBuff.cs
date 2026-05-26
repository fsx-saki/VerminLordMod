using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class LianHunBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = false;
            Main.debuff[Type] = false;
            BuffID.Sets.NurseCannotRemoveDebuff[Type] = false;
            Main.lightPet[Type] = false;
            Main.buffNoTimeDisplay[Type] = false;
            BuffID.Sets.LongerExpertDebuff[Type] = false;
            Main.pvpBuff[Type] = false;
            Main.persistentBuff[Type] = false;
            Main.vanityPet[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.GetModPlayer<LianHunPlayer>().HasLianHun = true;
            player.GetDamage(DamageClass.Generic) += 0.10f;
            player.GetCritChance(DamageClass.Generic) += 5f;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class LianHunPlayer : ModPlayer
    {
        public bool HasLianHun { get; set; }

        public override void ResetEffects()
        {
            HasLianHun = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasLianHun)
                return;

            if (Main.rand.NextFloat() < 0.10f)
            {
                target.AddBuff(BuffID.OnFire, 120);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasLianHun)
                return;

            if (Main.rand.NextFloat() < 0.10f)
            {
                target.AddBuff(BuffID.OnFire, 120);
            }
        }
    }
}
