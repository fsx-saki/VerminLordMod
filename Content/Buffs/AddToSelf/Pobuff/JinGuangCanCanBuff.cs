using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class JinGuangCanCanBuff : ModBuff
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
            player.GetDamage(DamageClass.Generic) += 0.10f;
            player.GetModPlayer<JinGuangCanCanPlayer>().HasJinGuangCanCan = true;

            Lighting.AddLight(player.Center, 1.0f, 0.95f, 0.5f);

            if (Main.rand.NextBool(4))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.GoldFlame);
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.8f, 1.2f);
            }
        }
    }

    public class JinGuangCanCanPlayer : ModPlayer
    {
        public bool HasJinGuangCanCan { get; set; }
        private const float DazzleRange = 200f;

        public override void ResetEffects()
        {
            HasJinGuangCanCan = false;
        }

        public override void PostUpdate()
        {
            if (!HasJinGuangCanCan)
                return;

            if (Player.whoAmI != Main.myPlayer)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= DazzleRange)
                {
                    npc.GetGlobalNPC<JinGuangCanCanNPC>().IsDazzled = true;
                }
            }
        }
    }

    public class JinGuangCanCanNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool IsDazzled { get; set; }

        public override void ResetEffects(NPC npc)
        {
            IsDazzled = false;
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            if (IsDazzled)
            {
                modifiers.DefenseEffectiveness *= 0.95f;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (IsDazzled)
            {
                modifiers.DefenseEffectiveness *= 0.95f;
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (IsDazzled)
            {
                drawColor = Color.Lerp(drawColor, Color.Gold, 0.25f);
            }
        }
    }
}
