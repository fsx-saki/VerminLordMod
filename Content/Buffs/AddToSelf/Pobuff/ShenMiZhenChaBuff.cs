using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class ShenMiZhenChaBuff : ModBuff
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
            player.GetModPlayer<ShenMiZhenChaPlayer>().HasShenMiZhenCha = true;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(player.position, player.width, player.height,
                    ModContent.DustType<TimeDust>());
                d.velocity *= 0.2f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.5f, 0.9f);
            }
        }
    }

    public class ShenMiZhenChaPlayer : ModPlayer
    {
        public bool HasShenMiZhenCha { get; set; }

        public override void ResetEffects()
        {
            HasShenMiZhenCha = false;
        }

        public override void PostUpdate()
        {
            if (!HasShenMiZhenCha)
                return;

            Player.detectCreature = true;
            Player.nightVision = true;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly)
                {
                    int mapX = (int)(npc.Center.X / 16f);
                    int mapY = (int)(npc.Center.Y / 16f);
                    if (mapX >= 0 && mapX < Main.maxTilesX && mapY >= 0 && mapY < Main.maxTilesY)
                    {
                        Main.Map.UpdateLighting(mapX, mapY, (byte)Math.Min(255, Main.Map[mapX, mapY].Light + 60));
                    }
                }
            }
        }
    }

    public class ShenMiZhenChaGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!npc.boss || npc.life <= 0)
                return;

            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<ShenMiZhenChaPlayer>();
            if (!modPlayer.HasShenMiZhenCha)
                return;

            float lifeRatio = (float)npc.life / npc.lifeMax;
            Vector2 barPos = npc.Center - screenPos;
            barPos.Y -= npc.height / 2f + 24f;

            int barWidth = 80;
            int barHeight = 6;

            Texture2D pixelTexture = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            spriteBatch.Draw(pixelTexture, new Rectangle((int)barPos.X - barWidth / 2 - 1, (int)barPos.Y - 1, barWidth + 2, barHeight + 2), new Color(20, 20, 20, 180));

            Color hpColor = lifeRatio > 0.5f ? new Color(50, 220, 50, 220) :
                            lifeRatio > 0.25f ? new Color(220, 220, 50, 220) :
                            new Color(220, 50, 50, 220);

            spriteBatch.Draw(pixelTexture, new Rectangle((int)barPos.X - barWidth / 2, (int)barPos.Y, (int)(barWidth * lifeRatio), barHeight), hpColor);
        }
    }
}
