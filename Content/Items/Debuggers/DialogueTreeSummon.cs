// ============================================================
// DialogueTreeSummon - 对话树演示NPC召唤令
// 左键使用：在鼠标位置生成 DialogueTreeDemoNPC
// 用于测试对话树系统
// ============================================================
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content.NPCs;

namespace VerminLordMod.Content.Items.Debuggers;

/// <summary>
/// 对话树演示NPC召唤令
/// 在鼠标位置生成一个 DialogueTreeDemoNPC，用于测试对话树系统
/// </summary>
public class DialogueTreeSummon : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.rare = ItemRarityID.Master;
        Item.maxStack = 99;
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
        if (player.whoAmI != Main.myPlayer)
            return true;

        // 在鼠标位置生成 NPC
        Vector2 spawnPos = Main.MouseWorld;
        int npcIndex = NPC.NewNPC(
            new EntitySource_DebugCommand("DialogueTreeSummon"),
            (int)spawnPos.X,
            (int)spawnPos.Y,
            ModContent.NPCType<DialogueTreeDemoNPC>()
        );

        if (npcIndex >= 0 && npcIndex < Main.maxNPCs)
        {
            NPC npc = Main.npc[npcIndex];
            npc.velocity = Vector2.Zero;
            npc.netUpdate = true;

            Main.NewText(
                $"[对话树调试] 已生成演示NPC: {npc.TypeName} (ID:{npc.whoAmI})",
                Color.Cyan
            );
        }
        else
        {
            Main.NewText("[对话树调试] NPC生成失败！", Color.Red);
        }

        return true;
    }

    public override bool AltFunctionUse(Player player) => false;

    public override void AddRecipes()
    {
        // 调试物品，无合成配方
    }
}
