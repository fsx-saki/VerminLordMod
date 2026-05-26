using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ZhenHunBuff : ModBuff
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
            player.GetModPlayer<ZhenHunPlayer>().ZhenHunActive = true;

            if (Main.rand.NextBool(6))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Ghost);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
                d.color = new Color(150, 130, 255);
            }
        }
    }

    public class ZhenHunPlayer : ModPlayer
    {
        public bool ZhenHunActive { get; set; }
        private const int AuraRadius = 500;
        private int auraTimer;

        public override void ResetEffects()
        {
            ZhenHunActive = false;
        }

        public override void PostUpdateEquips()
        {
            if (!ZhenHunActive)
            {
                auraTimer = 0;
                return;
            }

            Player.statDefense += 15;
            Player.buffImmune[BuffID.Confused] = true;

            if (Player.whoAmI != Main.myPlayer)
                return;

            auraTimer++;
            if (auraTimer >= 30)
            {
                auraTimer = 0;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (!npc.active || npc.friendly)
                        continue;

                    float dist = Vector2.Distance(npc.Center, Player.Center);
                    if (dist <= AuraRadius)
                    {
                        if (!npc.HasBuff(BuffID.Weak))
                            npc.AddBuff(BuffID.Weak, 60);
                        npc.GetGlobalNPC<ZhenHunNPC>().ZhenHunWeakened = true;
                    }
                }
            }
        }

        public override void UpdateDead()
        {
            ZhenHunActive = false;
        }
    }

    public class ZhenHunNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public bool ZhenHunWeakened { get; set; }

        public override void ResetEffects(NPC npc)
        {
            ZhenHunWeakened = false;
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (ZhenHunWeakened)
            {
                modifiers.FinalDamage *= 0.85f;
            }
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (ZhenHunWeakened)
            {
                modifiers.FinalDamage *= 0.85f;
            }
        }
    }
}
