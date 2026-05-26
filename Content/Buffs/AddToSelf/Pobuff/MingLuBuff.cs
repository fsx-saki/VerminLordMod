using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.Dusts;

namespace VerminLordMod.Content.Buffs.AddToSelf.Pobuff
{
    public class MingLuBuff : ModBuff
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
            player.GetModPlayer<MingLuPlayer>().HasMingLu = true;
        }
    }

    public class MingLuPlayer : ModPlayer
    {
        public bool HasMingLu { get; set; }

        public override void ResetEffects()
        {
            HasMingLu = false;
        }

        public override void PostUpdate()
        {
            if (!HasMingLu)
                return;

            if (Main.rand.NextBool(8))
            {
                var d = Dust.NewDustDirect(Player.position, Player.width, Player.height,
                    ModContent.DustType<WisdomDust>());
                d.velocity *= 0.3f;
                d.noGravity = true;
                d.scale = Main.rand.NextFloat(0.6f, 1.0f);
            }
        }
    }

    public class MingLuGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (!Main.player[Main.myPlayer].GetModPlayer<MingLuPlayer>().HasMingLu)
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

            string name = npc.TypeName;
            string hpText = $"{npc.life}/{npc.lifeMax}";
            Vector2 namePos = barPos - origin + new Vector2(0, -16f);
            Utils.DrawBorderString(spriteBatch, name, namePos, Color.LightYellow, 0.8f, 0f, 0f);
            Vector2 hpPos = barPos - origin + new Vector2(0, barHeight + 2f);
            Utils.DrawBorderString(spriteBatch, hpText, hpPos, Color.White * 0.9f, 0.7f, 0f, 0f);
        }
    }
}
