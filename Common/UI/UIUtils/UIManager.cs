// ============================================================
// UIManager - UI 管理器（ModSystem）
// 参考自 MiroonOS_Public (https://github.com/MiroonOS/MiroonOS_Public)
// 管理所有 BaseUI 实例的生命周期、点击检测、更新和绘制
// ============================================================
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace VerminLordMod.Common.UI.UIUtils
{
    /// <summary>
    /// UI 管理器。作为 ModSystem 自动加载，管理所有 BaseUI 实例的更新和绘制。
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class UIManager : ModSystem
    {
        /// <summary>
        /// 所有注册的 BaseUI 实例
        /// </summary>
        public static List<BaseUI> BaseUIs = [];

        // 悬停和点击的按钮引用
        public static BaseButton HoverButton = null;
        public static BaseButton ClickedButton = null;

        // 鼠标状态追踪
        private static bool LastLeftUnPressed = false;
        /// <summary>
        /// 当前帧是否发生了左键点击
        /// </summary>
        public static bool LeftClicked = false;
        private static bool LastRightUnPressed = false;
        /// <summary>
        /// 当前帧是否发生了右键点击
        /// </summary>
        public static bool RightClicked = false;

        public override void Load()
        {
            BaseUIs = [];
        }

        public override void Unload()
        {
            BaseUIs = null;
        }

        /// <summary>
        /// 在 PostDrawInterface 中绘制所有 UI
        /// </summary>
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            foreach (var baseUI in BaseUIs)
            {
                baseUI?.DrawSubPanels(spriteBatch);
            }
        }

        /// <summary>
        /// 检测点击事件（上升沿检测）
        /// </summary>
        public static bool DetectClick(ref bool lastUnPressed, bool currentRelease)
        {
            if (currentRelease)
            {
                lastUnPressed = true;
                return false;
            }
            else
            {
                bool clicked = lastUnPressed;
                lastUnPressed = false;
                return clicked;
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            LeftClicked = DetectClick(ref LastLeftUnPressed, Main.mouseLeftRelease);
            RightClicked = DetectClick(ref LastRightUnPressed, Main.mouseRightRelease);

            foreach (var baseUI in BaseUIs)
            {
                foreach (var basePanel in baseUI.SubPanels)
                {
                    UpdatePanels(basePanel);
                    foreach (var baseButton in basePanel.SubButtonsList)
                    {
                        if (baseButton == null) continue;
                        if (baseButton.ButtonActive && baseButton.ButtonVisible)
                        {
                            UpdateButtons(baseButton);
                            if (baseButton.ButtonCanDraw)
                            {
                                bool isClicked = baseButton.Clicked();
                                bool isHovered = baseButton.Hovered();

                                if (isClicked || isHovered)
                                {
                                    if (isClicked)
                                    {
                                        ClickedButton = baseButton;
                                        baseButton.ButtonClickedEven?.Invoke();
                                        if (baseButton.ButtonPlaySound != null)
                                        {
                                            SoundEngine.PlaySound(new SoundStyle(baseButton.ButtonPlaySound) { Volume = 1f }, Main.LocalPlayer.Center);
                                        }
                                    }

                                    if (isHovered)
                                    {
                                        HoverButton = baseButton;
                                        baseButton.LastHove = baseButton.Hovered();
                                    }

                                    goto FoundButton;
                                }
                            }
                        }
                    }
                }
            }

        FoundButton:;
        }

        /// <summary>
        /// 更新面板
        /// </summary>
        public static void UpdatePanels(BasePanel basePanel)
        {
            if (basePanel != null && basePanel.Active && basePanel.Visible)
            {
                if (basePanel.PreUpdate != null && !basePanel.PreUpdate.Invoke())
                    return;

                basePanel.Update?.Invoke();
                basePanel.PostUpdate?.Invoke();
            }
        }

        /// <summary>
        /// 更新按钮
        /// </summary>
        public static void UpdateButtons(BaseButton baseButton)
        {
            if (baseButton != null && baseButton.ButtonActive && baseButton.ButtonVisible)
            {
                if (baseButton.PreUpdate != null && !baseButton.PreUpdate.Invoke())
                    return;

                baseButton.Update?.Invoke();
                baseButton.PostUpdate?.Invoke();
            }
        }

        /// <summary>
        /// 创建一个新的 UI
        /// </summary>
        public static BaseUI NewUI(string UIName)
        {
            if (FindUI(UIName) != null)
            {
                Console.WriteLine($"UIManager 错误：已存在同名 UI \"{UIName}\"");
                return null;
            }
            BaseUI baseUI = new()
            {
                UIName = UIName
            };
            BaseUIs.Add(baseUI);
            return baseUI;
        }

        /// <summary>
        /// 按名称查找 UI
        /// </summary>
        public static BaseUI FindUI(string UIName)
        {
            foreach (BaseUI baseUI in BaseUIs)
            {
                if (baseUI is not null && baseUI.UIName == UIName)
                {
                    return baseUI;
                }
            }
            return null;
        }
    }
}
