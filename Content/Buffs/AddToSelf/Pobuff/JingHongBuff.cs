using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JingHongBuff : ModBuff
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
            player.GetModPlayer<JingHongPlayer>().HasJingHong = true;

            player.GetDamage(DamageClass.Generic) += 0.25f;
            player.GetCritChance(DamageClass.Generic) += 15;
            player.GetAttackSpeed(DamageClass.Generic) += 0.20f;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.RedTorch);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class JingHongPlayer : ModPlayer
    {
        public bool HasJingHong { get; set; }
        public Vector2 CastPosition { get; set; } = Vector2.Zero;
        private const float ArenaRadius = 400f;

        public override void ResetEffects()
        {
            HasJingHong = false;
        }

        public override void PostUpdate()
        {
            if (!HasJingHong || Player.whoAmI != Main.myPlayer)
                return;

            float dist = Vector2.Distance(Player.Center, CastPosition);
            if (dist > ArenaRadius)
            {
                Vector2 direction = (CastPosition - Player.Center).SafeNormalize(Vector2.Zero);
                Player.Center = CastPosition - direction * ArenaRadius;
                Player.velocity *= -0.3f;

                CombatText.NewText(Player.Hitbox, new Color(255, 80, 80), "领域边界!", false);
            }

            if (Main.rand.NextBool(8))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                var pos = CastPosition + new Vector2(
                    (float)System.Math.Cos(angle) * ArenaRadius,
                    (float)System.Math.Sin(angle) * ArenaRadius
                );
                var d = Dust.NewDustDirect(pos - new Vector2(4, 4), 8, 8, DustID.RedTorch);
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                d.velocity *= 0.1f;
            }
        }
    }
}
