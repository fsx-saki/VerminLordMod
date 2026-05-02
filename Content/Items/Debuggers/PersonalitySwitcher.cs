// ============================================================
// PersonalitySwitcher - 性格切换器
// 左键使用：循环切换目标NPC的性格（Aggressive -> Cautious -> Greedy -> Proud -> Benevolent）
// 右键使用：重置为默认性格（Aggressive）
// 用于测试对话树中基于性格的条件分支
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
/// 性格切换器
/// 左键循环切换NPC性格，右键重置为默认
/// </summary>
public class PersonalitySwitcher : ModItem
{
    private static readonly GuPersonality[] PersonalityCycle = new[]
    {
        GuPersonality.Aggressive,
        GuPersonality.Cautious,
        GuPersonality.Greedy,
        GuPersonality.Proud,
        GuPersonality.Benevolent,
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

        // 右键（altFunctionUse == 2）重置为 Aggressive，左键循环切换
        int direction = player.altFunctionUse == 2 ? 0 : +1;
        CyclePersonality(target, direction);
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

    private static void CyclePersonality(NPC npc, int direction)
    {
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            GuPersonality current = guMaster.GetPersonality();
            int currentIndex = System.Array.IndexOf(PersonalityCycle, current);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex;
            if (direction == 0)
            {
                newIndex = 0; // 重置为 Aggressive
            }
            else
            {
                newIndex = (currentIndex + direction + PersonalityCycle.Length) % PersonalityCycle.Length;
            }

            GuPersonality newPersonality = PersonalityCycle[newIndex];

            // 通过反射或直接设置（取决于GuMasterBase的实现）
            // GuMasterBase 的 Personality 是通过 GetPersonality() 获取的
            // 这里我们假设可以设置
            SetPersonality(guMaster, newPersonality);

            Main.NewText(
                $"[性格切换] {npc.TypeName} 性格: {GuiEnumHelper.GetPersonalityName(current)} -> {GuiEnumHelper.GetPersonalityName(newPersonality)}",
                GuiEnumHelper.GetPersonalityColor(newPersonality)
            );
        }
        else if (npc.ModNPC is NPCs.DialogueTreeDemoNPC demoNPC)
        {
            GuPersonality current = demoNPC.DebugPersonality;
            int currentIndex = System.Array.IndexOf(PersonalityCycle, current);
            if (currentIndex < 0) currentIndex = 0;

            int newIndex;
            if (direction == 0)
            {
                newIndex = 0;
            }
            else
            {
                newIndex = (currentIndex + direction + PersonalityCycle.Length) % PersonalityCycle.Length;
            }

            GuPersonality newPersonality = PersonalityCycle[newIndex];
            demoNPC.DebugPersonality = newPersonality;

            Main.NewText(
                $"[性格切换] 演示NPC性格: {GuiEnumHelper.GetPersonalityName(current)} -> {GuiEnumHelper.GetPersonalityName(newPersonality)}",
                GuiEnumHelper.GetPersonalityColor(newPersonality)
            );
        }
        else
        {
            Main.NewText("此NPC不支持性格切换。", Color.Red);
        }
    }

    /// <summary>
    /// 设置GuMasterBase的性格
    /// 通过反射访问私有字段 _personality
    /// </summary>
    private static void SetPersonality(GuMasterBase guMaster, GuPersonality personality)
    {
        // GuMasterBase 使用私有字段 _personality
        var field = typeof(GuMasterBase).GetField(
            "_personality",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            field.SetValue(guMaster, personality);
        }
        else
        {
            Main.NewText("无法设置性格：找不到 _personality 字段", Color.Red);
        }
    }

    public override void AddRecipes()
    {
        // 调试物品，无合成配方
    }
}
