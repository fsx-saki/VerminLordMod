using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class SanLianBuff : ModBuff
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
            player.GetModPlayer<SanLianPlayer>().HasSanLian = true;

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 40f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, ModContent.DustType<WarDust>());
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class SanLianPlayer : ModPlayer
    {
        public bool HasSanLian { get; set; }
        public int HitCount { get; set; }
        private const int MaxHits = 3;
        private const float DamageBonusPerHit = 0.15f;

        public override void ResetEffects()
        {
            HasSanLian = false;
        }

        public float GetCurrentDamageBonus()
        {
            if (!HasSanLian || HitCount >= MaxHits)
                return 0f;
            return DamageBonusPerHit * (HitCount + 1);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float bonus = GetCurrentDamageBonus();
            if (bonus > 0f)
            {
                modifiers.FinalDamage += bonus;
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)
        {
            float bonus = GetCurrentDamageBonus();
            if (bonus > 0f)
            {
                modifiers.FinalDamage += bonus;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasSanLian || HitCount >= MaxHits)
                return;

            HitCount++;
            if (HitCount >= MaxHits)
            {
                Player.ClearBuff(ModContent.BuffType<SanLianBuff>());
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasSanLian || HitCount >= MaxHits)
                return;

            HitCount++;
            if (HitCount >= MaxHits)
            {
                Player.ClearBuff(ModContent.BuffType<SanLianBuff>());
            }
        }
    }
}
