using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using Terraria.GameContent;

namespace VerminLordMod.Common.Players
{
    public class YingGanPlayer : ModPlayer
    {
        public bool HasYingGan { get; set; }

        public override void ResetEffects()
        {
            HasYingGan = false;
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (HasYingGan && Main.rand.NextFloat() < 0.20f)
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (HasYingGan && Main.rand.NextFloat() < 0.20f)
            {
                modifiers.FinalDamage *= 0.5f;
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            if (HasYingGan && hurtInfo.Damage < hurtInfo.SourceDamage)
            {
                SpawnYingGanEffect();
            }
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            if (HasYingGan && hurtInfo.Damage < hurtInfo.SourceDamage)
            {
                SpawnYingGanEffect();
            }
        }

        private void SpawnYingGanEffect()
        {
            CombatText.NewText(Player.getRect(), new Color(255, 165, 0), "硬干！");

            for (int i = 0; i < 10; i++)
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height, ModContent.DustType<PowerDust>());
                d.velocity = Main.rand.NextVector2Circular(2f, 2f);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }
        }
    }
}
