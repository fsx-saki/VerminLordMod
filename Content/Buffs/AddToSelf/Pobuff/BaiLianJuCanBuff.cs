using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class BaiLianJuCanBuff : ModBuff
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
            player.GetModPlayer<BaiLianJuCanPlayer>().HasBaiLianJuCan = true;
            player.GetDamage(DamageClass.Generic) += 0.05f;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Silk);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class BaiLianJuCanPlayer : ModPlayer
    {
        public bool HasBaiLianJuCan { get; set; }
        private const int SlowDuration = 120;
        private const float SlowChance = 0.15f;

        public override void ResetEffects()
        {
            HasBaiLianJuCan = false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasBaiLianJuCan && Main.rand.NextFloat() < SlowChance)
            {
                target.AddBuff(BuffID.Chilled, SlowDuration);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasBaiLianJuCan && Main.rand.NextFloat() < SlowChance)
            {
                target.AddBuff(BuffID.Chilled, SlowDuration);
            }
        }
    }
}
