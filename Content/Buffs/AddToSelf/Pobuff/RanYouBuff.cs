using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class RanYouBuff : ModBuff
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
            player.GetModPlayer<RanYouPlayer>().HasRanYou = true;
            player.GetDamage(DamageClass.Generic) += 0.30f;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Torch);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }
        }
    }

    public class RanYouPlayer : ModPlayer
    {
        public bool HasRanYou { get; set; }

        public override void ResetEffects()
        {
            HasRanYou = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasRanYou)
                return;

            for (int i = 0; i < target.buffType.Length; i++)
            {
                if (target.buffType[i] == BuffID.OnFire && target.buffTime[i] > 0)
                {
                    target.buffTime[i] *= 2;
                }
                if (target.buffType[i] == BuffID.OnFire3 && target.buffTime[i] > 0)
                {
                    target.buffTime[i] *= 2;
                }
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasRanYou)
                return;

            for (int i = 0; i < target.buffType.Length; i++)
            {
                if (target.buffType[i] == BuffID.OnFire && target.buffTime[i] > 0)
                {
                    target.buffTime[i] *= 2;
                }
                if (target.buffType[i] == BuffID.OnFire3 && target.buffTime[i] > 0)
                {
                    target.buffTime[i] *= 2;
                }
            }
        }
    }
}
