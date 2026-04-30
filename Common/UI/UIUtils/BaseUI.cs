// ============================================================
// BaseUI - 基础 UI 类
// 参考自 MiroonOS_Public (https://github.com/MiroonOS/MiroonOS_Public)
// 管理子面板列表，提供面板的添加、查找、绘制功能
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace VerminLordMod.Common.UI.UIUtils
{
    /// <summary>
    /// 基础 UI 类。管理一组子面板（BasePanel），提供面板的添加、查找、绘制功能。
    /// </summary>
    public class BaseUI
    {
        /// <summary>
        /// 子面板列表
        /// </summary>
        public List<BasePanel> SubPanels = [];

        /// <summary>
        /// UI 名称
        /// </summary>
        public string UIName;

        /// <summary>
        /// 绘制所有子面板及其按钮
        /// </summary>
        public void DrawSubPanels(SpriteBatch spriteBatch)
        {
            foreach (BasePanel basePanel in SubPanels)
            {
                if (basePanel != null)
                {
                    if (basePanel.CanDraw)
                    {
                        basePanel.Draw();
                    }
                    basePanel?.DrawSubButton(spriteBatch);
                    basePanel.DrawOther?.Invoke(spriteBatch);
                }
            }
        }

        /// <summary>
        /// 添加一个新面板
        /// </summary>
        public BasePanel NewPanel(Texture2D texture, float PanleRot, Vector2 Pos, Vector2 Size,
            bool Visible, bool CanDraw, bool FullScreen, string PanelName, Color DrawColor, Action Draw)
        {
            if (FindSubPanels(PanelName) != null)
            {
                Console.WriteLine($"BaseUI[{UIName}] 错误：已存在同名面板 \"{PanelName}\"");
                return null;
            }
            BasePanel basePanel = new()
            {
                Active = true,
                CanDraw = CanDraw,
                FullScreen = FullScreen,
                Panelcolor = DrawColor,
                PanelSize = Size,
                Visible = Visible,
                PanelCenter = Pos,
                PanelName = PanelName,
                PanelRot = PanleRot,
                PanelTex = texture,
                DrawAct = Draw
            };
            SubPanels.Add(basePanel);
            return basePanel;
        }

        /// <summary>
        /// 按名称查找子面板
        /// </summary>
        public BasePanel FindSubPanels(string PanelName)
        {
            foreach (BasePanel basePanel in SubPanels)
            {
                if (basePanel is not null && basePanel.PanelName == PanelName)
                {
                    return basePanel;
                }
            }
            return null;
        }
    }
}
