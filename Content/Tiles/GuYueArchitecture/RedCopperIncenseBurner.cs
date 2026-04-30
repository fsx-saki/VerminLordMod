using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    /// <summary>
    /// 赤铜香炉 - 摆放在黑漆台案两侧，香烟袅袅
    /// 原文: "牌位两侧摆着赤铜香炉，香烟袅袅" (gzr_part001.txt:116)
    /// 功能: 装饰性家具，发光并产生烟雾粒子效果
    /// </summary>
    public class RedCopperIncenseBurner : ModTile
    {
        private Asset<Texture2D> flameTexture;

        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1xX);
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
            TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.WaterDeath = true;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(180, 80, 40), Language.GetText("Mods.VerminLordMod.MapObject.RedCopperIncenseBurner"));

            flameTexture = ModContent.Request<Texture2D>(Texture + "_Flame");
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX == 0)
            {
                r = 0.8f;
                g = 0.4f;
                b = 0.1f;
            }
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (Main.gamePaused || !Main.instance.IsActive || Lighting.UpdateEveryFrame && !Main.rand.NextBool(4))
                return;

            Tile tile = Main.tile[i, j];
            if (!TileDrawing.IsVisible(tile))
                return;

            short frameX = tile.TileFrameX;
            short frameY = tile.TileFrameY;

            // Only emit smoke from the top tile
            if (frameX != 0 || frameY % 36 != 0 || !Main.rand.NextBool(3))
                return;

            // 香烟袅袅 - 烟雾粒子
            var dust = Dust.NewDustDirect(
                new Vector2(i * 16 + 4, j * 16 - 4), 4, 4,
                DustID.Smoke, 0f, -0.5f, 100, default, 1.2f);
            dust.noGravity = true;
            dust.velocity *= 0.3f;
            dust.velocity.Y = dust.velocity.Y - 1.2f;
            dust.alpha = 100;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (!TileDrawing.IsVisible(tile))
                return;

            // Only draw flame on top tile
            if (tile.TileFrameY % 36 != 0)
                return;

            Vector2 zero = new Vector2(Main.offScreenRange, Main.offScreenRange);
            if (Main.drawToScreen)
                zero = Vector2.Zero;

            int width = 16;
            int offsetY = 0;
            int height = 16;
            short frameX = tile.TileFrameX;
            short frameY = tile.TileFrameY;

            TileLoader.SetDrawPositions(i, j, ref width, ref offsetY, ref height, ref frameX, ref frameY);

            ulong randSeed = Main.TileFrameSeed ^ (ulong)((long)j << 32 | (long)(uint)i);

            for (int c = 0; c < 5; c++)
            {
                float shakeX = Utils.RandomInt(ref randSeed, -8, 9) * 0.15f;
                float shakeY = Utils.RandomInt(ref randSeed, -4, 5) * 0.15f;

                spriteBatch.Draw(
                    flameTexture.Value,
                    new Vector2(
                        i * 16 - (int)Main.screenPosition.X - (width - 16f) / 2f + shakeX,
                        j * 16 - (int)Main.screenPosition.Y + offsetY + shakeY) + zero,
                    new Rectangle(frameX, frameY, width, height),
                    new Color(180, 100, 30, 0), 0f, default, 1f, SpriteEffects.None, 0f);
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32,
                ModContent.ItemType<Items.Placeable.GuYueArchitecture.RedCopperIncenseBurner>());
        }
    }
}
