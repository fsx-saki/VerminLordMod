using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.WolfWaveUI
{
    /// <summary>
    /// 狼潮进度条 — 现代化扁平轻量风格
    /// 仅在狼潮事件激活时显示
    /// </summary>
    class WolfWaveBar : UIState
    {
        private UIText text;
        private UIElement area;

        public override void OnInitialize()
        {
            area = new UIElement();
            area.Left.Set(-600, 1f);
            area.Top.Set(70, 0f);
            area.Width.Set(200, 0f);
            area.Height.Set(48, 0f);

            text = new UIText("狼潮进度：0%", 0.8f);
            text.Width.Set(200, 0f);
            text.Height.Set(24, 0f);
            text.Top.Set(28, 0f);
            text.Left.Set(0, 0f);
            text.TextColor = UIStyles.TextMain;

            area.Append(text);
            Append(area);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!WolfSystem.isWolfWave)
                return;
            base.Draw(spriteBatch);
        }

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            float quotient = WolfSystem.WolfWaveRate / 100;
            quotient = Utils.Clamp(quotient, 0, 1);

            var bgRect = area.GetInnerDimensions().ToRectangle();
            bgRect.Width = 200;
            bgRect.Height = 24;

            Color fillA = UIStyles.TextDanger;
            Color fillB = new Color(160, 60, 60);

            UIRendering.DrawProgressBar(spriteBatch, bgRect, quotient,
                UIStyles.WolfBarBg, UIStyles.WolfBarBorder, fillA, fillB);
        }

        public override void Update(GameTime gameTime)
        {
            int per = (int)WolfSystem.WolfWaveRate;
            text.SetText($"狼潮进度：{per}%");
            base.Update(gameTime);
        }
    }

    [Autoload(Side = ModSide.Client)]
    internal class WolfWaveUISystem : ModSystem
    {
        private UserInterface WolfWaveUserInterface;
        internal WolfWaveBar WolfWaveBar;
        public static LocalizedText WolfWaveText;

        public override void Load()
        {
            WolfWaveBar = new();
            WolfWaveUserInterface = new();
            WolfWaveUserInterface.SetState(WolfWaveBar);

            string category = "UI";
            WolfWaveText ??= Mod.GetLocalization($"{category}.WolfWave");
        }

        public override void UpdateUI(GameTime gameTime)
        {
            WolfWaveUserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int index = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (index != -1)
            {
                layers.Insert(index, new LegacyGameInterfaceLayer(
                    "VerminLordMod: Wolf Wave Bar",
                    delegate
                    {
                        WolfWaveUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
