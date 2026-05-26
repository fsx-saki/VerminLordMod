using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class HunDengBuff : ModBuff
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
            player.GetModPlayer<HunDengPlayer>().HasHunDeng = true;
            player.GetDamage(DamageClass.Generic) += 0.10f;

            if (Main.rand.NextBool(5))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height, DustID.Ghost);
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class HunDengPlayer : ModPlayer
    {
        public bool HasHunDeng { get; set; }
        private const int GlowRange = 600;

        public override void ResetEffects()
        {
            HasHunDeng = false;
        }

        public override void PostUpdate()
        {
            if (!HasHunDeng)
                return;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.active || npc.friendly)
                    continue;

                float dist = Vector2.Distance(Player.Center, npc.Center);
                if (dist <= GlowRange)
                {
                    npc.GetGlobalNPC<HunDengNPC>().IsGlowing = true;
                }
            }
        }
    }

    public class HunDengNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool IsGlowing { get; set; }

        public override void ResetEffects(NPC npc)
        {
            IsGlowing = false;
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (IsGlowing)
            {
                drawColor = Color.Lerp(drawColor, Color.White, 0.5f);
            }
        }

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (IsGlowing)
            {
                Vector2 drawPos = npc.Center - screenPos;
                float pulse = 0.8f + 0.2f * (float)System.Math.Sin(Main.GameUpdateCount * 0.05f);
                Lighting.AddLight(npc.Center, new Vector3(0.4f * pulse, 0.3f * pulse, 0.6f * pulse));
            }
        }
    }
}
