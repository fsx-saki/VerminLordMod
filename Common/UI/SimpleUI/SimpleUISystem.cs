// ============================================================
// SimpleUISystem - 完全独立设计的UI系统注册
// 不依赖任何现有UI系统（ModernUI等）
// 管理 SimplePanel / SimpleInfoBox / SearchChairItem 的生命周期、更新和绘制
// ============================================================
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using VerminLordMod.Content.Items.Debuggers.SearchChair;

namespace VerminLordMod.Common.UI.SimpleUI;

/// <summary>
/// SimpleUI 系统 — 管理 SimplePanel 和 SimpleInfoBox 实例
/// 注册到游戏界面层，处理更新和绘制
/// </summary>
[Autoload(Side = ModSide.Client)]
public class SimpleUISystem : ModSystem
{
    /// <summary> SimpleUI 面板实例 </summary>
    internal static SimplePanel? PanelInstance { get; private set; }

    /// <summary> SimpleUI 信息提示框实例 </summary>
    internal static SimpleInfoBox? InfoBoxInstance { get; private set; }

    /// <summary> 面板是否已初始化 </summary>
    private static bool _initialized;

    public override void Load()
    {
        if (!Main.dedServ)
        {
            _initialized = false;
        }
    }

    public override void Unload()
    {
        PanelInstance = null;
        InfoBoxInstance = null;
        SearchChairHandler.Unload();
    }

    public override void UpdateUI(GameTime gameTime)
    {
        // 延迟初始化：确保 Main.screenWidth/screenHeight 有效
        if (!_initialized && !Main.dedServ)
        {
            InitializePanel();
            _initialized = true;
        }

        // 更新面板
        PanelInstance?.Update();

        // 更新信息提示框
        InfoBoxInstance?.Update();

        // 更新 SearchChairHandler 的信息提示框和搜索面板
        SearchChairHandler.Update();
    }

    /// <summary>
    /// 初始化面板和信息提示框
    /// </summary>
    private static void InitializePanel()
    {
        int width = 360;
        int height = 320;
        PanelInstance = new SimplePanel(width, height)
        {
            Title = "SimpleUI 面板"
        };

        // ===== 创建信息提示框 =====
        InfoBoxInstance = new SimpleInfoBox(300, 180)
        {
            Title = "提示信息"
        };

        // 信息提示框的子面板
        var infoSubPanel = new SimpleSubPanel
        {
            RelativeX = 0.05f,
            RelativeY = 0.05f,
            RelativeWidth = 0.9f,
            RelativeHeight = 0.9f,
            Title = ""
        };

        infoSubPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Label,
            RelX = 0.05f,
            RelY = 0.1f,
            Width = 260,
            Height = 20,
            LabelText = "这是一个信息提示框",
            LabelColor = new Color(220, 200, 255),
            LabelScale = 0.85f
        });

        infoSubPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Label,
            RelX = 0.05f,
            RelY = 0.35f,
            Width = 260,
            Height = 20,
            LabelText = "无边框 · 无关闭按钮 · 无调整大小",
            LabelColor = new Color(200, 180, 240),
            LabelScale = 0.75f
        });

        infoSubPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Label,
            RelX = 0.05f,
            RelY = 0.55f,
            Width = 260,
            Height = 20,
            LabelText = "标题栏不可移动",
            LabelColor = new Color(180, 160, 220),
            LabelScale = 0.75f
        });

        InfoBoxInstance.SubPanels.Add(infoSubPanel);

        // ===== 创建动画格子行实例（供两个子面板共享） =====
        var animSlotRow = new SimpleAnimSlotRow();

        // ===== 创建测试子面板1：按钮 + 物品格子 =====
        var testSubPanel = new SimpleSubPanel
        {
            RelativeX = 0.05f,
            RelativeY = 0.05f,
            RelativeWidth = 0.9f,
            RelativeHeight = 0.45f,
            Title = "测试区域"
        };

        // 物品格子
        var slot = new SimpleItemSlot
        {
            SlotSize = 52
        };

        // 固定框：包含按钮和物品格子，整体弹性居中，内部间距固定
        var fixedGroup = new SimpleFixedGroup
        {
            Spacing = 12
        };

        // 按钮子元素
        fixedGroup.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Button,
            RelX = 0f,
            RelY = 0f,
            Width = 120,
            Height = 40,
            ButtonText = "放入土块",
            ButtonClick = () =>
            {
                // 找到子面板中的物品格子元素
                var panel = PanelInstance;
                if (panel == null) return;

                foreach (var sp in panel.SubPanels)
                {
                    foreach (var elem in sp.Elements)
                    {
                        if (elem.Type == SubPanelElementType.FixedGroup && elem.FixedGroup != null)
                        {
                            foreach (var child in elem.FixedGroup.Elements)
                            {
                                if (child.Type == SubPanelElementType.ItemSlot && child.ItemSlot != null)
                                {
                                    var itemSlot = child.ItemSlot;

                                    if (!itemSlot.IsEmpty)
                                    {
                                        var existing = itemSlot.StoredItem;
                                        if (existing != null && !existing.IsAir)
                                        {
                                            Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_GiftOrReward(), existing);
                                        }
                                        itemSlot.ClearSlot();
                                    }

                                    var dirt = new Item();
                                    dirt.SetDefaults(ItemID.DirtBlock);
                                    dirt.stack = 1;

                                    itemSlot.PutItem(dirt);
                                    Main.NewText("[SimpleUI] 向格子放入了一块土", new Color(200, 160, 255));

                                    // 显示信息提示框
                                    InfoBoxInstance?.Open();
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        });

        // 物品格子子元素
        fixedGroup.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.ItemSlot,
            RelX = (120 - 52) / 2f,
            RelY = 0f,
            Width = 52,
            Height = 52,
            ItemSlot = slot
        });

        fixedGroup.CalculateSize();

        var groupElem = new SubPanelElement
        {
            Type = SubPanelElementType.FixedGroup,
            RelX = 0.5f,
            RelY = 0.5f,
            AnchorX = ElementAnchor.Center,
            AnchorY = ElementAnchor.Center,
            Width = fixedGroup.Width,
            Height = fixedGroup.Height,
            FixedGroup = fixedGroup
        };

        testSubPanel.Elements.Add(groupElem);

        PanelInstance.SubPanels.Add(testSubPanel);

        // ===== 创建测试子面板2：动画展示区（深色背景） =====
        var animSubPanel = new SimpleSubPanel
        {
            RelativeX = 0.05f,
            RelativeY = 0.55f,
            RelativeWidth = 0.9f,
            RelativeHeight = 0.4f,
            Title = "动画展示"
        };

        // 使用固定框容纳播放/复位两个按钮，整体居中
        var btnFixedGroup = new SimpleFixedGroup
        {
            Spacing = 16
        };

        btnFixedGroup.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Button,
            RelX = 0f,
            RelY = 0f,
            Width = 80,
            Height = 30,
            ButtonText = "播放",
            ButtonClick = () =>
            {
                animSlotRow.Play();
                Main.NewText("[SimpleUI] 开始播放动画", new Color(200, 160, 255));
            }
        });

        btnFixedGroup.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Button,
            RelX = 0f,
            RelY = 0f,
            Width = 80,
            Height = 30,
            ButtonText = "复位",
            ButtonClick = () =>
            {
                animSlotRow.Reset();
                Main.NewText("[SimpleUI] 已复位", new Color(200, 160, 255));
            }
        });

        btnFixedGroup.CalculateSize();

        animSubPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.FixedGroup,
            RelX = 0.5f,
            RelY = 0.15f,
            AnchorX = ElementAnchor.Center,
            AnchorY = ElementAnchor.Center,
            Width = btnFixedGroup.Width,
            Height = btnFixedGroup.Height,
            FixedGroup = btnFixedGroup
        });

        // 动画格子行（占据子面板下半部分）
        animSubPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.AnimSlotRow,
            RelX = 0f,
            RelY = 0.4f,
            Width = 1,
            Height = 1,
            AnimRow = animSlotRow
        });

        PanelInstance.SubPanels.Add(animSubPanel);

        // ===== 创建测试子面板3：纯标签 =====
        var labelSubPanel = new SimpleSubPanel
        {
            RelativeX = 0.05f,
            RelativeY = 0.6f,
            RelativeWidth = 0.9f,
            RelativeHeight = 0.35f,
            Title = "信息"
        };

        labelSubPanel.Elements.Add(new SubPanelElement
        {
            Type = SubPanelElementType.Label,
            RelX = 0.05f,
            RelY = 0.1f,
            Width = 200,
            Height = 20,
            LabelText = "点击按钮放入土块，点击格子取走",
            LabelColor = new Color(200, 160, 255),
            LabelScale = 0.8f
        });

        PanelInstance.SubPanels.Add(labelSubPanel);
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        // 在鼠标文本层之前绘制 SimpleUI 面板和信息提示框
        int mouseTextIndex = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");
        if (mouseTextIndex != -1)
        {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                "VerminLordMod: SimpleUI Panel",
                () =>
                {
                    if (PanelInstance != null)
                    {
                        PanelInstance.Draw(Main.spriteBatch);
                    }

                    // 信息提示框绘制在面板之上
                    if (InfoBoxInstance != null)
                    {
                        InfoBoxInstance.Draw(Main.spriteBatch);
                    }

                    // 绘制 SearchChairHandler 的信息提示框和搜索面板
                    SearchChairHandler.Draw(Main.spriteBatch);

                    return true;
                },
                InterfaceScaleType.UI
            ));
        }
    }

    /// <summary>
    /// 切换面板显示/隐藏
    /// </summary>
    public static void TogglePanel()
    {
        PanelInstance?.Toggle();
    }
}
