using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JinGuBuff : ModBuff
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
            player.GetModPlayer<JinGuPlayer>().HasJinGu = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.MagicMirror);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class JinGuPlayer : ModPlayer
    {
        public bool HasJinGu { get; set; }

        public override void ResetEffects()
        {
            HasJinGu = false;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!HasJinGu)
                return;

            int bonusDamage = (int)(target.lifeMax * 0.05f);
            if (bonusDamage > 0)
            {
                modifiers.FinalDamage.Flat += bonusDamage;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasJinGu)
                return;

            for (int i = 0; i < 5; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.MagicMirror);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!HasJinGu)
                return;

            for (int i = 0; i < 5; i++)
            {
                var d = Dust.NewDustDirect(target.position, target.width, target.height, DustID.MagicMirror);
                d.noGravity = true;
                d.velocity *= 0.3f;
                d.scale = Main.rand.NextFloat(0.8f, 1.3f);
            }
        }
    }
}
