using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class TuiBeiBuff : ModBuff
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
            player.GetModPlayer<TuiBeiPlayer>().HasTuiBeiBuff = true;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Smoke);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.8f);
                d.alpha = 120;
            }
        }
    }

    public class TuiBeiPlayer : ModPlayer
    {
        public bool HasTuiBeiBuff { get; set; }
        private bool _counterAttackUsed;

        public override void ResetEffects()
        {
            if (!HasTuiBeiBuff)
                _counterAttackUsed = false;
            HasTuiBeiBuff = false;
        }

        public override void PostUpdateEquips()
        {
            if (!HasTuiBeiBuff || _counterAttackUsed)
                return;

            Player.GetDamage(DamageClass.Generic) += 0.15f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasTuiBeiBuff && !_counterAttackUsed)
            {
                _counterAttackUsed = true;
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasTuiBeiBuff && !_counterAttackUsed)
            {
                _counterAttackUsed = true;
            }
        }
    }
}
