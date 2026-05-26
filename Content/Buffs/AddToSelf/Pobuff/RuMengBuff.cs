using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class RuMengBuff : ModBuff
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
            player.GetModPlayer<RuMengPlayer>().HasRuMeng = true;

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 40f);
                var pos = player.Center + new Vector2((float)System.Math.Cos(angle) * dist, (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, ModContent.DustType<DreamDust>());
                d.noGravity = true;
                d.velocity *= 0.2f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class RuMengPlayer : ModPlayer
    {
        public bool HasRuMeng { get; set; }
        private static readonly int[] DebuffPool = {
            BuffID.OnFire,
            BuffID.Frostburn,
            BuffID.Ichor,
            BuffID.CursedInferno,
            BuffID.Electrified
        };
        private const float DebuffChance = 0.20f;
        private const int DebuffDuration = 120;

        public override void ResetEffects()
        {
            HasRuMeng = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasRuMeng)
                return;

            if (Main.rand.NextFloat() < DebuffChance)
            {
                int debuff = DebuffPool[Main.rand.Next(DebuffPool.Length)];
                target.AddBuff(debuff, DebuffDuration);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasRuMeng)
                return;

            if (Main.rand.NextFloat() < DebuffChance)
            {
                int debuff = DebuffPool[Main.rand.Next(DebuffPool.Length)];
                target.AddBuff(debuff, DebuffDuration);
            }
        }
    }
}
