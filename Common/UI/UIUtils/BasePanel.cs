// ============================================================
// BasePanel - 基础面板类
// 参考自 MiroonOS_Public (https://github.com/MiroonOS/MiroonOS_Public)
// 包含子按钮列表管理、绘制、查找等功能
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace VerminLordMod.Common.UI.UIUtils
{
    /// <summary>
    /// 基础面板类。包含子按钮列表，支持自定义绘制、全屏模式。
    /// </summary>
    public class BasePanel
    {
        /// <summary>
        /// 每帧更新函数
        /// </summary>
        public Action Update;

        /// <summary>
        /// 自定义绘制委托（若不为 null，则替代默认纹理绘制）
        /// </summary>
        public Action DrawAct;

        /// <summary>
        /// 额外绘制（接收 SpriteBatch 参数）
        /// </summary>
        public Action<SpriteBatch> DrawOther;

        /// <summary>
        /// 面板中心位置
        /// </summary>
        public Vector2 PanelCenter;

        /// <summary>
        /// 更新前函数，返回 false 则跳过本次更新
        /// </summary>
        public Func<bool> PreUpdate;

        /// <summary>
        /// 更新后函数
        /// </summary>
        public Action PostUpdate;

        /// <summary>
        /// 面板纹理
        /// </summary>
        public Texture2D PanelTex;

        /// <summary>
        /// 面板名称
        /// </summary>
        public string PanelName;

        /// <summary>
        /// 子按钮列表
        /// </summary>
        public List<BaseButton> SubButtonsList = [];

        /// <summary>
        /// 是否占满屏幕绘制
        /// </summary>
        public bool FullScreen = false;

        /// <summary>
        /// 面板颜色
        /// </summary>
        public Color Panelcolor;

        /// <summary>
        /// 面板是否启用（灰色效果）
        /// </summary>
        public bool Visible = true;

        /// <summary>
        /// 面板旋转角度
        /// </summary>
        public float PanelRot = 0;

        /// <summary>
        /// 面板是否可绘制
        /// </summary>
        public bool CanDraw = true;

        /// <summary>
        /// 面板是否活跃（false 则被销毁）
        /// </summary>
        public bool Active = true;

        /// <summary>
        /// 面板缩放大小
        /// </summary>
        public Vector2 PanelSize;

        /// <summary>
        /// 绘制面板
        /// </summary>
        public void Draw()
        {
            if (DrawAct != null)
            {
                DrawAct.Invoke();
                return;
            }

            if (PanelTex == null) return;

            Color color = Visible ? Panelcolor : Color.Gray;
            Texture2D texture = PanelTex;

            Vector2 drawPosition = new(PanelCenter.X, PanelCenter.Y);

            EasyDraw.AnotherDraw(BlendState.NonPremultiplied);
            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                null,
                color,
                PanelRot,
                new Vector2(texture.Width / 2, texture.Height / 2),
                PanelSize,
                SpriteEffects.None,
                1f
            );
            EasyDraw.AnotherDraw(BlendState.NonPremultiplied);
        }

        /// <summary>
        /// 绘制所有子按钮
        /// </summary>
        public void DrawSubButton(SpriteBatch spriteBatch)
        {
            foreach (BaseButton baseButton in SubButtonsList)
            {
                if (baseButton != null && baseButton.ButtonActive && baseButton.ButtonCanDraw)
                {
                    baseButton.Draw();
                    baseButton.DrawOther?.Invoke(spriteBatch);
                }
            }
        }

        /// <summary>
        /// 按名称查找子按钮
        /// </summary>
        public BaseButton FindSubButton(string ButtonName)
        {
            foreach (BaseButton baseButton in SubButtonsList)
            {
                if (baseButton is not null && baseButton.ButtonName == ButtonName)
                {
                    return baseButton;
                }
            }
            return null;
        }

        /// <summary>
        /// 添加一个新的子按钮
        /// </summary>
        public BaseButton NewButton(Vector2 ButtonCenter, Vector2 Size, Color color, string Name,
            bool CanDraw, bool Visible, string PlayerSound, float Rot,
            Texture2D texture, Texture2D texture_Hover, Action Draw)
        {
            if (FindSubButton(Name) is not null)
            {
                Console.WriteLine($"BasePanel[{PanelName}] 错误：已存在同名按钮 \"{Name}\"");
                return null;
            }

            BaseButton baseButton = new()
            {
                ButtonActive = true,
                ButtonCanDraw = CanDraw,
                ButtonVisible = Visible,
                ButtonName = Name,
                ButtonSize = Size,
                ButtonColor = color,
                ButtonPlaySound = PlayerSound,
                ButtonRot = Rot,
                ButtonTex = texture,
                ButtonTex_Hover = texture_Hover,
                ButtonCenter = ButtonCenter,
                DrawAct = Draw
            };
            SubButtonsList.Add(baseButton);
            return baseButton;
        }
    }
}
