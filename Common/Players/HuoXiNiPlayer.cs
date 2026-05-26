using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace VerminLordMod.Common.Players
{
    public class HuoXiNiPlayer : ModPlayer
    {
        public bool HuoXiNiActive { get; set; }
        public bool HasMudShield { get; set; }
        private int _shieldCooldown = 0;
        private const int CooldownFrames = 900;
        private const int DamageThreshold = 40;

        public override void ResetEffects()
        {
            HuoXiNiActive = false;
            if (_shieldCooldown > 0)
                _shieldCooldown--;
        }

        public override void PostUpdateEquips()
        {
            if (!HuoXiNiActive)
            {
                HasMudShield = false;
                return;
            }

            if (HasMudShield && Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(20f, 35f);
                var pos = Player.Center + new Microsoft.Xna.Framework.Vector2(
                    (float)System.Math.Cos(angle) * dist,
                    (float)System.Math.Sin(angle) * dist);
                var d = Dust.NewDustDirect(pos - new Microsoft.Xna.Framework.Vector2(4, 4), 8, 8,
                    ModContent.DustType<global::VerminLordMod.Content.Dusts.MudDust>());
                d.noGravity = true;
                d.velocity *= 0.1f;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }

        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            TryActivateShield(hurtInfo.SourceDamage);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            TryActivateShield(hurtInfo.SourceDamage);
        }

        private void TryActivateShield(int damage)
        {
            if (!HuoXiNiActive || _shieldCooldown > 0)
                return;

            if (damage > DamageThreshold)
            {
                HasMudShield = true;
                _shieldCooldown = CooldownFrames;

                for (int i = 0; i < 15; i++)
                {
                    var d = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                        ModContent.DustType<global::VerminLordMod.Content.Dusts.MudDust>());
                d.noGravity = true;
                d.velocity = new Microsoft.Xna.Framework.Vector2(
                    Main.rand.NextFloat(-3f, 3f),
                    Main.rand.NextFloat(-3f, 3f));
                d.scale = Main.rand.NextFloat(1.0f, 1.5f);
            }

            CombatText.NewText(Player.Hitbox, new Microsoft.Xna.Framework.Color(139, 90, 43), "泥盾生成！");
        }
    }

    public override void ModifyHurt(ref Player.HurtModifiers modifiers)
    {
        if (HasMudShield)
        {
            modifiers.FinalDamage *= 0f;
            Player.immuneTime = (int)(60);
            HasMudShield = false;

            for (int i = 0; i < 20; i++)
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                    ModContent.DustType<global::VerminLordMod.Content.Dusts.MudDust>());
                    d.noGravity = true;
                    d.velocity = new Microsoft.Xna.Framework.Vector2(
                        Main.rand.NextFloat(-4f, 4f),
                        Main.rand.NextFloat(-4f, 4f));
                    d.scale = Main.rand.NextFloat(1.2f, 1.8f);
                }

                CombatText.NewText(Player.Hitbox, new Microsoft.Xna.Framework.Color(139, 90, 43), "泥盾破碎！");
            }
        }
    }
}
