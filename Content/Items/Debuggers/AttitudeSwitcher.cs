// ============================================================
// AttitudeSwitcher - 态度切换器
// 左键使用：循环切换目标NPC的态度（Ignore -> Wary -> Hostile -> Friendly -> Respectful -> Fearful -> Contemptuous）
// 右键使用：重置为默认态度（Ignore）
// 用于测试对话树中基于态度的条件分支
// ============================================================
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;
using VerminLordMod.Common.UI.UIUtils;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Content.Items.Debuggers;

/// <summary>
/// 态度切换器
/// 左键循环切换NPC态度，右键重置为默认
/// </summary>
public class AttitudeSwitcher : ModItem
{
    private static readonly GuAttitude[] AttitudeCycle = new[]
    {
        GuAttitude.Ignore,
        GuAttitude.Wary,
        GuAttitude.Hostile,
        GuAttitude.Friendly,
        GuAttitude.Respectful,
        GuAttitude.Fearful,
        GuAttitude.Contemptuous,
    };

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.rare = ItemRarityID.Master;
        Item.maxStack = 1;
        Item.value = 100;

        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.Guitar;
        Item.autoReuse = true;
        Item.useTurn = true;
        Item.UseSound = SoundID.Item1;
    }

    public override bool? UseItem(Player player)
    {
        NPC target = GetHoveredNPC();
        if (target == null)
        {
            Main.NewText("请将鼠标对准一个NPC后使用此道具。", Color.Yellow);
            return true;
        }

        // 右键（altFunctionUse == 2）重置为 Ignore，左键循环切换
        int direction = player.altFunctionUse == 2 ? 0 : +1;
        CycleAttitude(target, direction);
        return true;
    }

    public override bool AltFunctionUse(Player player) => true;

    private static NPC GetHoveredNPC()
    {
        Vector2 mouseWorld = Main.MouseWorld;

        for (int i = 0; i < Main.maxNPCs; i++)
        {
            NPC npc = Main.npc[i];
            if (npc.active && npc.getRect().Contains((int)mouseWorld.X, (int)mouseWorld.Y))
            {
                return npc;
            }
        }

        return null;
    }

    private static void CycleAttitude(NPC npc, int direction)
    {
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            GuAttitude current = guMaster.CurrentAttitude;
            int currentIndex = System.Array.IndexOf(AttitudeCycle, current);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex;
            if (direction == 0)
            {
                newIndex = 0; // 重置为 Ignore
            }
            else
            {
                newIndex = (currentIndex + direction + AttitudeCycle.Length) % AttitudeCycle.Length;
            }

            GuAttitude newAttitude = AttitudeCycle[newIndex];
            guMaster.CurrentAttitude = newAttitude;

            string attitudeName = GuiEnumHelper.GetAttitudeName(newAttitude);
            Main.NewText(
                $"[态度切换] {npc.TypeName} 态度: {GuiEnumHelper.GetAttitudeName(current)} -> {attitudeName}",
                GuiEnumHelper.GetAttitudeColor(newAttitude)
            );
        }
        else if (npc.ModNPC is NPCs.DialogueTreeDemoNPC demoNPC)
        {
            GuAttitude current = demoNPC.DebugAttitude;
            int currentIndex = System.Array.IndexOf(AttitudeCycle, current);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex;
            if (direction == 0)
            {
                newIndex = 0;
            }
            else
            {
                newIndex = (currentIndex + direction + AttitudeCycle.Length) % AttitudeCycle.Length;
            }

            GuAttitude newAttitude = AttitudeCycle[newIndex];
            demoNPC.DebugAttitude = newAttitude;

            Main.NewText(
                $"[态度切换] 演示NPC态度: {GuiEnumHelper.GetAttitudeName(current)} -> {GuiEnumHelper.GetAttitudeName(newAttitude)}",
                GuiEnumHelper.GetAttitudeColor(newAttitude)
            );
        }
        else
        {
            Main.NewText("此NPC不支持态度切换。", Color.Red);
        }
    }

    public override void AddRecipes()
    {
        // 调试物品，无合成配方
    }
}
