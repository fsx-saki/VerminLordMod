using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ShiJueBuff : ModBuff
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
            player.GetModPlayer<ShiJuePlayer>().ShiJueActive = true;
            player.GetDamage(DamageClass.Generic) += 0.20f;

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.PurpleTorch);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class ShiJuePlayer : ModPlayer
    {
        public bool ShiJueActive { get; set; }
        private const float FormationRadius = 500f;
        private const int DamagePerSecond = 15;
        private int damageTimer;

        public override void ResetEffects()
        {
            ShiJueActive = false;
        }

        public override void PostUpdate()
        {
            if (!ShiJueActive)
            {
                damageTimer = 0;
                return;
            }

            if (Player.whoAmI != Main.myPlayer)
                return;

            damageTimer++;
            if (damageTimer >= 60)
            {
                damageTimer = 0;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(npc.Center, Player.Center);
                    if (dist <= FormationRadius)
                    {
                        npc.SimpleStrikeNPC(DamagePerSecond, 0, false, 0f, DamageClass.Default, true, Player.luck);
                        npc.GetGlobalNPC<ShiJueNPC>().ShiJueDefenseReduction = true;
                    }
                }
            }

            if (Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat() * FormationRadius;
                Vector2 pos = Player.Center + new Vector2(
                    (float)System.Math.Cos(angle) * radius,
                    (float)System.Math.Sin(angle) * radius
                );
                var d = Dust.NewDustDirect(pos, 4, 4, DustID.PurpleTorch);
                d.noGravity = true;
                d.velocity *= 0.1f;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class ShiJueNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool ShiJueDefenseReduction { get; set; }

        public override void ResetEffects(NPC npc)
        {
            ShiJueDefenseReduction = false;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (ShiJueDefenseReduction)
            {
                modifiers.DefenseEffectiveness *= 0.8f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (ShiJueDefenseReduction)
            {
                modifiers.DefenseEffectiveness *= 0.8f;
            }
        }
    }
}
