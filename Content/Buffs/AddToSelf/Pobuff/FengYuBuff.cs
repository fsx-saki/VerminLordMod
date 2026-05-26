using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class FengYuBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.15f;
            player.GetModPlayer<FengYuPlayer>().HasFengYu = true;
        }
    }

    public class FengYuPlayer : ModPlayer
    {
        public bool HasFengYu { get; set; }

        public override void ResetEffects()
        {
            HasFengYu = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasFengYu && Main.rand.NextFloat() < 0.20f)
            {
                target.AddBuff(BuffID.Chilled, 120);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasFengYu && Main.rand.NextFloat() < 0.20f)
            {
                target.AddBuff(BuffID.Chilled, 120);
            }
        }
    }
}
