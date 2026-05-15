using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.UI.UIUtils;

namespace VerminLordMod.Common.UI.QiUI
{
    /// <summary>
    /// 真元/仙元条 — 现代化扁平轻量风格
    /// 显示当前真元/仙元值、境界、资质信息
    /// </summary>
    public class QiBar : UIState
    {
        private UIText text;
        private UIText text2;
        private UIElement area;

        public override void OnInitialize()
        {
            area = new UIElement();
            area.Left.Set(-600, 1f);
            area.Top.Set(30, 0f);
            area.Width.Set(200, 0f);
            area.Height.Set(72, 0f);

            text = new UIText("未开元海", 0.8f);
            text.Width.Set(200, 0f);
            text.Height.Set(24, 0f);
            text.Top.Set(44, 0f);
            text.Left.Set(0, 0f);
            text.TextColor = UIStyles.TextMain;

            text2 = new UIText("", 0.7f);
            text2.Width.Set(200, 0f);
            text2.Height.Set(20, 0f);
            text2.Top.Set(64, 0f);
            text2.Left.Set(0, 0f);
            text2.TextColor = UIStyles.TextSecondary;

            area.Append(text);
            area.Append(text2);
            Append(area);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        string yuan = "真元";
        string qiao = "空窍";

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            base.DrawSelf(spriteBatch);

            var qiResource = Main.LocalPlayer.GetModPlayer<QiResourcePlayer>();
            var qiRealm = Main.LocalPlayer.GetModPlayer<QiRealmPlayer>();

            float quotient = qiResource.QiMaxCurrent == 0 ? 0 : (float)qiResource.QiCurrent / qiResource.QiMaxCurrent;
            quotient = Utils.Clamp(quotient, 0f, 1f);

            var bgRect = area.GetInnerDimensions().ToRectangle();
            bgRect.Width = 200;
            bgRect.Height = 36;

            Color fillA, fillB;
            switch (qiRealm.GuLevel)
            {
                case 1: fillA = new Color(130, 215, 130); fillB = new Color(80, 170, 80); break;
                case 2: fillA = new Color(200, 130, 130); fillB = new Color(160, 80, 80); break;
                case 3: fillA = new Color(130, 150, 170); fillB = new Color(80, 100, 120); break;
                case 4: fillA = new Color(220, 210, 90); fillB = new Color(180, 170, 60); break;
                case 5: fillA = new Color(180, 110, 200); fillB = new Color(140, 70, 160); break;
                case 6: fillA = new Color(100, 220, 180); fillB = new Color(60, 180, 140); break;
                case 7: fillA = new Color(230, 150, 160); fillB = new Color(190, 100, 110); break;
                case 8: fillA = new Color(100, 150, 190); fillB = new Color(60, 110, 150); break;
                case 9: fillA = new Color(240, 180, 60); fillB = new Color(200, 140, 30); break;
                case 10: fillA = new Color(220, 220, 230); fillB = new Color(180, 180, 190); break;
                default: fillA = UIStyles.TextDim; fillB = new Color(60, 60, 70); break;
            }

            UIRendering.DrawProgressBar(spriteBatch, bgRect, quotient,
                UIStyles.QiBarBg, UIStyles.QiBarBorder, fillA, fillB);
        }

        public override void Update(GameTime gameTime)
        {
            var qiResource = Main.LocalPlayer.GetModPlayer<QiResourcePlayer>();
            var qiRealm = Main.LocalPlayer.GetModPlayer<QiRealmPlayer>();
            var qiTalent = Main.LocalPlayer.GetModPlayer<QiTalentPlayer>();

            string str;
            int rr = qiResource.QiMaxCurrent == 0 ? -1 : (int)(qiResource.QiCurrent * 10 / qiResource.QiMaxCurrent);
            if (qiRealm.GuLevel <= 0)
                rr = -1;
            if (qiRealm.GuLevel >= 6)
            {
                yuan = "仙元";
                qiao = "仙窍";
            }
            else
            {
                yuan = "真元";
                qiao = "空窍";
            }
            str = rr switch
            {
                -1 => "未开元海",
                0 => yuan + "耗尽",
                1 => "一成" + yuan,
                2 => "二成" + yuan,
                3 => "三成" + yuan,
                4 => "四成" + yuan,
                5 => "五成" + yuan,
                6 => "六成" + yuan,
                7 => "七成" + yuan,
                8 => "八成" + yuan,
                9 => "九成" + yuan,
                10 => "十成" + yuan,
                _ => "未知" + yuan,
            };
            text.SetText(qiao + "：" + str + " [" + qiResource.QiCurrent + "/" + qiResource.QiMaxCurrent + "]");

            string str2 = qiRealm.LevelStage switch
            {
                0 => "初期",
                1 => "中期",
                2 => "后期",
                3 => "巅峰",
                _ => "",
            };

            string str3 = qiTalent.Grade switch
            {
                QiTalentPlayer.TalentGrade.Jia => "甲等",
                QiTalentPlayer.TalentGrade.Yi => "乙等",
                QiTalentPlayer.TalentGrade.Bing => "丙等",
                QiTalentPlayer.TalentGrade.Ding => "丁等",
                _ => "未知",
            };

            text2.SetText($"资质：{str3}  境界：{qiRealm.GuLevel}转{str2}");
            base.Update(gameTime);
        }
    }

    [Autoload(Side = ModSide.Client)]
    public class ExampleResourceUISystem : ModSystem
    {
        private UserInterface QiBarUserInterface;
        public QiBar QiBar;
        public static LocalizedText QiText { get; private set; }

        public override void Load()
        {
            QiBar = new();
            QiBarUserInterface = new();
            QiBarUserInterface.SetState(QiBar);

            string category = "UI";
            QiText ??= Mod.GetLocalization($"{category}.Qi");
        }

        public override void UpdateUI(GameTime gameTime)
        {
            QiBarUserInterface?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: Qi Bar",
                    delegate
                    {
                        QiBarUserInterface.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI)
                );
            }
        }
    }
}
