using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class MingPaiBuff : ModBuff
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
            player.GetModPlayer<MingPaiPlayer>().HasMingPai = true;
        }
    }

    public class MingPaiPlayer : ModPlayer
    {
        public bool HasMingPai { get; set; }

        public override void ResetEffects()
        {
            HasMingPai = false;
        }
    }

    public class MingPaiGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!Main.player[Main.myPlayer].GetModPlayer<MingPaiPlayer>().HasMingPai)
                return;

            if (!npc.active || npc.friendly)
                return;

            float lifeRatio = (float)npc.life / npc.lifeMax;
            if (lifeRatio <= 0f)
                return;

            Vector2 barPos = npc.Center - screenPos + new Vector2(0, npc.height / 2f + 10f);
            int barWidth = Math.Max(30, npc.width);
            int barHeight = 4;

            Vector2 origin = new Vector2(barWidth / 2f, barHeight / 2f);

            Texture2D pixelTexture = Terraria.GameContent.TextureAssets.MagicPixel.Value;

            spriteBatch.Draw(pixelTexture, barPos - origin, new Rectangle(0, 0, barWidth, barHeight), Color.Black * 0.7f, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

            Color barColor = lifeRatio > 0.5f ? Color.Green : lifeRatio > 0.25f ? Color.Yellow : Color.Red;
            int filledWidth = (int)(barWidth * lifeRatio);
            if (filledWidth > 0)
            {
                spriteBatch.Draw(pixelTexture, barPos - origin, new Rectangle(0, 0, filledWidth, barHeight), barColor * 0.85f, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
            }

            if (npc.boss)
            {
                float bossLifeRatio = (float)npc.life / npc.lifeMax;
                string bossName = npc.TypeName;
                string bossHpText = $"{bossName}: {npc.life}/{npc.lifeMax}";
                Vector2 textPos = new Vector2(Main.screenWidth / 2f, 60f);
                Vector2 textSize = FontAssets.MouseText.Value.MeasureString(bossHpText);
                Utils.DrawBorderString(spriteBatch, bossHpText, textPos, Color.Orange, 1f, 0.5f, 0f);

                int bossBarWidth = 300;
                int bossBarHeight = 8;
                Vector2 bossBarPos = new Vector2(Main.screenWidth / 2f, 80f);
                Vector2 bossBarOrigin = new Vector2(bossBarWidth / 2f, bossBarHeight / 2f);

                spriteBatch.Draw(pixelTexture, bossBarPos - bossBarOrigin, new Rectangle(0, 0, bossBarWidth, bossBarHeight), Color.Black * 0.7f, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);

                int bossFilledWidth = (int)(bossBarWidth * bossLifeRatio);
                if (bossFilledWidth > 0)
                {
                    spriteBatch.Draw(pixelTexture, bossBarPos - bossBarOrigin, new Rectangle(0, 0, bossFilledWidth, bossBarHeight), Color.Orange * 0.9f, 0f, Vector2.Zero, Vector2.One, SpriteEffects.None, 0f);
                }
            }
        }
    }
}
