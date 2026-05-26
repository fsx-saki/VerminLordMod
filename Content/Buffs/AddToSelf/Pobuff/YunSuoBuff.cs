using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class YunSuoBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.08f;
            player.GetModPlayer<YunSuoPlayer>().HasYunSuo = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<CloudDust>());
                d.velocity = new Vector2(0, -0.8f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class YunSuoPlayer : ModPlayer
    {
        public bool HasYunSuo { get; set; }
        private const float ChilledChance = 0.15f;
        private const int ChilledDuration = 120;

        public override void ResetEffects()
        {
            HasYunSuo = false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyCloudLock(target);
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            ApplyCloudLock(target);
        }

        private void ApplyCloudLock(NPC target)
        {
            if (!HasYunSuo) return;

            if (Main.rand.NextFloat() < ChilledChance)
            {
                target.AddBuff(BuffID.Chilled, ChilledDuration);
            }
        }
    }
}
