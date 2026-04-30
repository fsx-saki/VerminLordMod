// ============================================================
// BaseButton - 基础按钮类
// 参考自 MiroonOS_Public (https://github.com/MiroonOS/MiroonOS_Public)
// 提供 hover/click 检测、自定义绘制委托、声音播放等功能
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace VerminLordMod.Common.UI.UIUtils
{
    /// <summary>
    /// 基础按钮类。支持 hover/click 检测、自定义绘制、声音播放。
    /// </summary>
    public class BaseButton
    {
        /// <summary>
        /// 按钮点击事件委托
        /// </summary>
        public Action ButtonClickedEven = null;

        /// <summary>
        /// 自定义绘制委托（若不为 null，则替代默认纹理绘制）
        /// </summary>
        public Action DrawAct;

        /// <summary>
        /// 每帧更新函数
        /// </summary>
        public Action Update;

        /// <summary>
        /// 更新前函数，返回 false 则跳过本次更新
        /// </summary>
        public Func<bool> PreUpdate;

        /// <summary>
        /// 额外绘制（接收 SpriteBatch 参数）
        /// </summary>
        public Action<SpriteBatch> DrawOther;

        /// <summary>
        /// 更新后函数
        /// </summary>
        public Action PostUpdate;

        /// <summary>
        /// 按钮中心位置
        /// </summary>
        public Vector2 ButtonCenter;

        /// <summary>
        /// 按钮点击时播放的声音路径
        /// </summary>
        public string ButtonPlaySound;

        /// <summary>
        /// 正常状态纹理
        /// </summary>
        public Texture2D ButtonTex;

        /// <summary>
        /// 悬停状态纹理
        /// </summary>
        public Texture2D ButtonTex_Hover;

        /// <summary>
        /// 按钮缩放大小
        /// </summary>
        public Vector2 ButtonSize = new(1f, 1f);

        /// <summary>
        /// 按钮名称
        /// </summary>
        public string ButtonName;

        /// <summary>
        /// 按钮是否可见（灰色效果）
        /// </summary>
        public bool ButtonVisible = true;

        /// <summary>
        /// 按钮是否活跃（false 则被销毁）
        /// </summary>
        public bool ButtonActive;

        /// <summary>
        /// 按钮是否可绘制
        /// </summary>
        public bool ButtonCanDraw;

        /// <summary>
        /// 绘制颜色
        /// </summary>
        public Color ButtonColor;

        /// <summary>
        /// 旋转角度
        /// </summary>
        public float ButtonRot = 0f;

        /// <summary>
        /// 上一帧是否悬停
        /// </summary>
        public bool LastHove;

        /// <summary>
        /// 检测鼠标是否悬停在按钮上
        /// </summary>
        public bool Hovered()
        {
            if (!ButtonCanDraw || ButtonTex == null)
                return false;

            Rectangle buttonRectangle = new(
                (int)(ButtonCenter.X - ButtonTex.Width / 2),
                (int)(ButtonCenter.Y - ButtonTex.Height / 2),
                ButtonTex.Width,
                ButtonTex.Height
            );
            Point mousePosition = new(Main.mouseX, Main.mouseY);

            return buttonRectangle.Contains(mousePosition);
        }

        /// <summary>
        /// 绘制按钮
        /// </summary>
        public void Draw()
        {
            if (DrawAct != null)
            {
                DrawAct.Invoke();
                return;
            }

            if (ButtonTex == null) return;

            Color color = ButtonVisible ? ButtonColor : Color.Gray;
            Texture2D texture = Hovered() ? (ButtonTex_Hover ?? ButtonTex) : ButtonTex;

            Vector2 drawPosition = new(ButtonCenter.X, ButtonCenter.Y);

            EasyDraw.AnotherDraw(BlendState.NonPremultiplied);
            Main.spriteBatch.Draw(
                texture,
                drawPosition,
                null,
                color,
                ButtonRot,
                new Vector2(texture.Width / 2, texture.Height / 2),
                ButtonSize,
                SpriteEffects.None,
                1f
            );
            EasyDraw.AnotherDraw(BlendState.NonPremultiplied);
        }

        /// <summary>
        /// 检查按钮是否被点击
        /// </summary>
        public bool Clicked()
        {
            if (!ButtonVisible || !ButtonActive)
                return false;

            if (Hovered() && UIManager.LeftClicked)
            {
                Main.LocalPlayer.controlUseItem = false;
                return true;
            }
            return false;
        }
    }
}
