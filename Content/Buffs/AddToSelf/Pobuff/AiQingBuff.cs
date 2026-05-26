using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;
using Terraria.GameContent;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class AiQingBuff : ModBuff
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
            player.GetModPlayer<AiQingPlayer>().HasAiQing = true;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, ModContent.DustType<LoveDust>());
                d.velocity *= 0.3f;
                d.velocity.Y -= 0.5f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class AiQingPlayer : ModPlayer
    {
        public bool HasAiQing { get; set; }
        public bool HasLinkedTarget { get; set; }
        public bool SoloEmpower { get; set; }
        public bool LinkedIsNpc { get; set; }
        public int LinkedWhoAmI { get; set; } = -1;
        public int LinkedNpcIndex { get; set; } = -1;

        public override void ResetEffects()
        {
            HasAiQing = false;
            HasLinkedTarget = false;
            SoloEmpower = false;
            LinkedIsNpc = false;
            LinkedWhoAmI = -1;
            LinkedNpcIndex = -1;
        }

        public override void PostUpdate()
        {
            if (!HasAiQing)
                return;

            if (SoloEmpower)
            {
                Player.GetDamage(DamageClass.Generic) += 0.30f;
            }

            if (HasLinkedTarget && Player.whoAmI == Main.myPlayer)
            {
                if (!LinkedIsNpc && LinkedWhoAmI >= 0 && LinkedWhoAmI < Main.maxPlayers)
                {
                    Player other = Main.player[LinkedWhoAmI];
                    if (other.active && !other.dead)
                    {
                        if (Main.rand.NextBool(8))
                        {
                            Vector2 mid = (Player.Center + other.Center) / 2f;
                            var d = Dust.NewDustDirect(mid - new Vector2(4, 4), 8, 8, ModContent.DustType<LoveDust>());
                            d.noGravity = true;
                            d.velocity *= 0.2f;
                            d.scale = Main.rand.NextFloat(0.5f, 0.8f);
                        }
                    }
                }
            }
        }

        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (!HasAiQing || !HasLinkedTarget)
                return;

            float redirect = modifiers.FinalDamage.ApplyTo(1f) * 0.5f;

            if (!LinkedIsNpc && LinkedWhoAmI >= 0 && LinkedWhoAmI < Main.maxPlayers)
            {
                Player other = Main.player[LinkedWhoAmI];
                if (other.active && !other.dead)
                {
                    modifiers.FinalDamage *= 0.5f;
                    other.Hurt(Terraria.DataStructures.PlayerDeathReason.LegacyDefault(), (int)redirect, 0);
                }
            }
        }

        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (!HasAiQing || !HasLinkedTarget)
                return;

            float redirect = modifiers.FinalDamage.ApplyTo(1f) * 0.5f;

            if (!LinkedIsNpc && LinkedWhoAmI >= 0 && LinkedWhoAmI < Main.maxPlayers)
            {
                Player other = Main.player[LinkedWhoAmI];
                if (other.active && !other.dead)
                {
                    modifiers.FinalDamage *= 0.5f;
                    other.Hurt(Terraria.DataStructures.PlayerDeathReason.LegacyDefault(), (int)redirect, 0);
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasAiQing && SoloEmpower)
            {
                int heal = (int)(damageDone * 0.05f);
                if (heal > 0)
                    Player.Heal(heal);
            }
        }

        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (HasAiQing && SoloEmpower)
            {
                int heal = (int)(damageDone * 0.05f);
                if (heal > 0)
                    Player.Heal(heal);
            }
        }
    }
}
