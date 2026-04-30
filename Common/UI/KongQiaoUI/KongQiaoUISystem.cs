using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace VerminLordMod.Common.UI.KongQiaoUI
{
    /// <summary>
    /// 空窍 UI 系统 — 管理空窍入口按钮和空窍面板的显示/隐藏和绘制
    /// 
    /// 使用方式：
    /// - 自动注册到游戏界面层
    /// - 空窍入口按钮常驻显示，可拖动
    /// - 点击按钮通过 ToggleUI() 切换空窍面板显示/隐藏
    /// - 按钮位置通过 KongQiaoToggleSavePlayer 自动保存/恢复
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class KongQiaoUISystem : ModSystem
    {
        private UserInterface _kongQiaoPanelUI;
        private UserInterface _toggleUI;
        internal KongQiaoUI KongQiaoUIInstance;
        internal GuCraftUI GuCraftUIInstance;
        internal KongQiaoToggle KongQiaoToggleInstance;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                // 初始化合成系统
                GuCraftSystem.Initialize();

                // 空窍面板
                _kongQiaoPanelUI = new UserInterface();
                KongQiaoUIInstance = new KongQiaoUI();
                KongQiaoUIInstance.Activate();

                // 合炼面板
                GuCraftUIInstance = new GuCraftUI();
                GuCraftUIInstance.Activate();

                // 空窍入口按钮（常驻）
                _toggleUI = new UserInterface();
                KongQiaoToggleInstance = new KongQiaoToggle();
                KongQiaoToggleInstance.Activate();
                _toggleUI.SetState(KongQiaoToggleInstance);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _toggleUI?.Update(gameTime);
            _kongQiaoPanelUI?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            // 空窍入口按钮 — 在资源条层绘制
            int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
            if (resourceBarIndex != -1)
            {
                layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: KongQiao Toggle",
                    () =>
                    {
                        _toggleUI?.Draw(Main.spriteBatch, new GameTime());
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }

            // 空窍面板 — 在鼠标文本层之前绘制
            int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: KongQiao Panel",
                    () =>
                    {
                        if (_kongQiaoPanelUI?.CurrentState != null)
                        {
                            _kongQiaoPanelUI.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }

        /// <summary> 切换空窍面板的显示/隐藏 </summary>
        public void ToggleUI()
        {
            if (_kongQiaoPanelUI.CurrentState == null)
            {
                KongQiaoUIInstance.Refresh();
                _kongQiaoPanelUI.SetState(KongQiaoUIInstance);
            }
            else
            {
                _kongQiaoPanelUI.SetState(null);
            }
        }

        /// <summary> 切换到合炼面板 </summary>
        public void ToggleGuCraftUI()
        {
            if (_kongQiaoPanelUI.CurrentState == null)
            {
                _kongQiaoPanelUI.SetState(GuCraftUIInstance);
            }
            else if (_kongQiaoPanelUI.CurrentState == GuCraftUIInstance)
            {
                _kongQiaoPanelUI.SetState(null);
            }
            else
            {
                _kongQiaoPanelUI.SetState(GuCraftUIInstance);
            }
        }

        /// <summary> 关闭合炼面板 </summary>
        public void CloseGuCraftUI()
        {
            if (_kongQiaoPanelUI.CurrentState == GuCraftUIInstance)
            {
                _kongQiaoPanelUI.SetState(null);
            }
        }

        /// <summary> 切换回空窍面板 </summary>
        public void ShowKongQiaoUI()
        {
            KongQiaoUIInstance.Refresh();
            _kongQiaoPanelUI.SetState(KongQiaoUIInstance);
        }

        /// <summary>
        /// 恢复按钮位置（由 KongQiaoToggleSavePlayer 在加载存档后调用）
        /// </summary>
        public void RestoreTogglePosition(float x, float y)
        {
            KongQiaoToggleInstance?.LoadPosition(x, y);
        }
    }
}
