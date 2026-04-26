using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace VerminLordMod.Common.UI.ReputationUI
{
    /// <summary>
    /// 声望 UI 系统 - 管理 ReputationUI 的显示/隐藏和绘制
    /// 
    /// 使用方式：
    /// - 自动注册到游戏界面层
    /// - 通过 ReputationUISystem.ToggleUI() 切换显示/隐藏
    /// - 绑定到快捷键或物品右键
    /// </summary>
    [Autoload(Side = ModSide.Client)]
    public class ReputationUISystem : ModSystem
    {
        private UserInterface _reputationUI;
        internal ReputationUI ReputationUIInstance;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                _reputationUI = new UserInterface();
                ReputationUIInstance = new ReputationUI();
                ReputationUIInstance.Activate();
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            _reputationUI?.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "VerminLordMod: Reputation UI",
                    () =>
                    {
                        if (_reputationUI?.CurrentState != null)
                        {
                            _reputationUI.Draw(Main.spriteBatch, new GameTime());
                        }
                        return true;
                    },
                    InterfaceScaleType.UI
                ));
            }
        }

        /// <summary> 切换声望 UI 的显示/隐藏 </summary>
        public void ToggleUI()
        {
            if (_reputationUI.CurrentState == null)
            {
                ReputationUIInstance.RefreshFactionList();
                _reputationUI.SetState(ReputationUIInstance);
            }
            else
            {
                _reputationUI.SetState(null);
            }
        }
    }
}
