using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using InnoVault;
using InnoVault.UIHandles;
using VerminLordMod.Content.Items.QuestItems;

namespace VerminLordMod.Common.QuestSystem
{
    public class QiHUD : UIHandle
    {
        public static QiHUD Instance => UIHandleLoader.GetUIHandleOfType<QiHUD>();
        public static bool ShowBar = true;

        public override bool Active => ShowBar;

        public Vector2 Position = new(20, 400);
        private bool isDragging;
        private Vector2 dragOffset;

        private const int BarWidth = 180;
        private const int BarHeight = 18;

        public override void Update()
        {
            if (!ShowBar) return;

            var rect = new Rectangle((int)Position.X, (int)Position.Y, BarWidth, BarHeight);
            bool hover = rect.Contains(Main.MouseScreen.ToPoint());

            if (hover && keyRightPressState == KeyPressState.Pressed && !isDragging)
            {
                isDragging = true;
                dragOffset = Main.MouseScreen - Position;
            }

            if (isDragging)
            {
                Position = Main.MouseScreen - dragOffset;
                if (keyRightPressState == KeyPressState.Released)
                    isDragging = false;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!ShowBar) return;

            var qi = Main.LocalPlayer.GetModPlayer<global::VerminLordMod.Common.Players.QiResourcePlayer>();
            if (qi == null || qi.QiMaxCurrent <= 0) return;

            var px = VaultAsset.placeholder2.Value;
            int x = (int)Position.X;
            int y = (int)Position.Y;

            spriteBatch.Draw(px, new Rectangle(x, y, BarWidth, BarHeight), new Color(20, 18, 25) * 0.85f);
            spriteBatch.Draw(px, new Rectangle(x, y, BarWidth, 1), new Color(60, 55, 70) * 0.5f);
            spriteBatch.Draw(px, new Rectangle(x, y + BarHeight - 1, BarWidth, 1), new Color(10, 10, 15) * 0.5f);

            float pct = qi.QiMaxCurrent > 0 ? qi.QiCurrent / qi.QiMaxCurrent : 0f;
            pct = MathHelper.Clamp(pct, 0f, 1f);
            int fillW = (int)((BarWidth - 4) * pct);
            if (fillW > 0)
            {
                var flags = Main.LocalPlayer.GetModPlayer<YuanHaiFlags>();
                Color fillColor = (flags != null && flags.IsJiaGrade) ? new Color(160, 100, 220) : new Color(100, 180, 220);
                spriteBatch.Draw(px, new Rectangle(x + 2, y + 2, fillW, BarHeight - 4), fillColor * 0.9f);
            }

            string text = $"真元 {(int)qi.QiCurrent}/{(int)qi.QiMaxCurrent}";
            var yhFlags = Main.LocalPlayer.GetModPlayer<YuanHaiFlags>();
            if (yhFlags != null && yhFlags.IsJiaGrade) text += " [甲]";
            Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, text,
                x + 8, y + 2, Color.White, Color.Black, Vector2.Zero, 0.7f);
        }
    }
}
