// ============================================================
// DialogueEffect - 对话效果系统
// 选择选项后触发的副作用
// ============================================================
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.Players;
using VerminLordMod.Common.Systems;
using VerminLordMod.Content.NPCs.GuMasters;

namespace VerminLordMod.Common.DialogueTree;

/// <summary>
/// 对话效果 — 选择选项后触发的副作用
/// </summary>
public abstract class DialogueEffect
{
    /// <summary> 执行效果 </summary>
    public abstract void Execute(Player player, NPC npc, BeliefState belief);

    /// <summary>
    /// 消耗玩家背包中指定类型的物品
    /// 替代 Player.ConsumeItem(int, bool) 的 API 差异
    /// </summary>
    protected static void ConsumeItemFromInventory(Player player, int itemType, int count)
    {
        int remaining = count;
        for (int i = 0; i < player.inventory.Length && remaining > 0; i++)
        {
            var item = player.inventory[i];
            if (item.type == itemType && item.stack > 0)
            {
                int take = System.Math.Min(item.stack, remaining);
                item.stack -= take;
                remaining -= take;
                if (item.stack <= 0)
                    item.TurnToAir();
            }
        }
    }
}

// ============================================================
// 具体效果类型
// ============================================================

/// <summary> 修改信念效果 </summary>
public class ModifyBeliefEffect : DialogueEffect
{
    public enum BeliefField { RiskThreshold, ConfidenceLevel, EstimatedPower }
    public enum ModifyOp { Set, Add, Multiply }

    public BeliefField Field;
    public ModifyOp Operation;
    public float Value;

    public ModifyBeliefEffect() { }

    public ModifyBeliefEffect(BeliefField field, ModifyOp op, float value)
    {
        Field = field;
        Operation = op;
        Value = value;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        if (belief == null) return;

        ref float fieldRef = ref GetFieldRef(belief);

        fieldRef = Operation switch
        {
            ModifyOp.Set => Value,
            ModifyOp.Add => fieldRef + Value,
            ModifyOp.Multiply => fieldRef * Value,
            _ => fieldRef
        };

        // 边界限制
        fieldRef = MathHelper.Clamp(fieldRef, 0f, 1f);
    }

    private ref float GetFieldRef(BeliefState belief)
    {
        switch (Field)
        {
            case BeliefField.RiskThreshold:
                return ref belief.RiskThreshold;
            case BeliefField.ConfidenceLevel:
                return ref belief.ConfidenceLevel;
            case BeliefField.EstimatedPower:
                return ref belief.EstimatedPower;
            default:
                return ref belief.RiskThreshold;
        }
    }
}

/// <summary> 修改声望效果 </summary>
public class ModifyReputationEffect : DialogueEffect
{
    public FactionID Faction;
    public int Amount;
    public string Reason;

    public ModifyReputationEffect() { }

    public ModifyReputationEffect(FactionID faction, int amount, string reason = "")
    {
        Faction = faction;
        Amount = amount;
        Reason = reason;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
        if (Amount >= 0)
            worldPlayer.AddReputation(Faction, Amount, Reason);
        else
            worldPlayer.RemoveReputation(Faction, -Amount, Reason);
    }
}

/// <summary> 给予物品效果 </summary>
public class GiveItemEffect : DialogueEffect
{
    public int ItemType;
    public int Stack = 1;

    public GiveItemEffect() { }

    public GiveItemEffect(int itemType, int stack = 1)
    {
        ItemType = itemType;
        Stack = stack;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        player.QuickSpawnItem(new EntitySource_DebugCommand("DialogueTree"), ItemType, Stack);
    }
}

/// <summary> 移除物品效果 </summary>
public class RemoveItemEffect : DialogueEffect
{
    public int ItemType;
    public int Stack = 1;

    public RemoveItemEffect() { }

    public RemoveItemEffect(int itemType, int stack = 1)
    {
        ItemType = itemType;
        Stack = stack;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        int removed = 0;
        for (int i = 0; i < player.inventory.Length && removed < Stack; i++)
        {
            var item = player.inventory[i];
            if (item.type == ItemType && item.stack > 0)
            {
                int take = System.Math.Min(item.stack, Stack - removed);
                item.stack -= take;
                removed += take;
                if (item.stack <= 0)
                    item.TurnToAir();
            }
        }
    }
}

/// <summary> 设置对话标记效果 </summary>
public class SetFlagEffect : DialogueEffect
{
    public string FlagName;
    public bool Value = true;

    public SetFlagEffect() { }

    public SetFlagEffect(string flagName, bool value = true)
    {
        FlagName = flagName;
        Value = value;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        // 标记存储在对话会话中，由 DialogueSession 处理
        // 这里仅做占位，实际由 DialogueTreeManager 处理
    }
}

/// <summary> 显示消息效果 </summary>
public class ShowMessageEffect : DialogueEffect
{
    public string Message;
    public Color MessageColor = Color.White;

    public ShowMessageEffect() { }

    public ShowMessageEffect(string message, Color? color = null)
    {
        Message = message;
        MessageColor = color ?? Color.White;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        Main.NewText(Message, MessageColor);
    }
}

/// <summary> 设置态度效果 </summary>
public class SetAttitudeEffect : DialogueEffect
{
    public GuAttitude Attitude;

    public SetAttitudeEffect() { }

    public SetAttitudeEffect(GuAttitude attitude)
    {
        Attitude = attitude;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            guMaster.CurrentAttitude = Attitude;
        }
    }
}

/// <summary> 重置NPC状态效果 </summary>
public class ResetStateEffect : DialogueEffect
{
    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        if (npc.ModNPC is GuMasterBase guMaster)
        {
            guMaster.CurrentAttitude = GuAttitude.Ignore;
            guMaster.HasBeenHitByPlayer = false;
            guMaster.AggroTimer = 0;
            guMaster.ProjectileProtectionEnabled = true;

            // 重置信念
            if (belief != null)
            {
                belief.RiskThreshold = 0.9f;
                belief.ConfidenceLevel = 0f;
                belief.EstimatedPower = 0.5f;
                belief.HasTraded = false;
                belief.HasFought = false;
                belief.WasDefeated = false;
                belief.HasDefeatedPlayer = false;
            }
        }
    }
}

/// <summary> 组合效果（同时执行多个效果） </summary>
public class CompositeEffect : DialogueEffect
{
    public DialogueEffect[] Effects;

    public CompositeEffect(params DialogueEffect[] effects)
    {
        Effects = effects;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        foreach (var effect in Effects)
        {
            effect.Execute(player, npc, belief);
        }
    }
}

// ============================================================
// 新增效果类型（阶段1扩展）
// ============================================================

/// <summary> 购买物品效果 - 消耗元石获得物品 </summary>
public class BuyItemEffect : DialogueEffect
{
    public int ItemType;
    public int PriceYuanS;
    public int Stack = 1;

    public BuyItemEffect() { }

    public BuyItemEffect(int itemType, int priceYuanS, int stack = 1)
    {
        ItemType = itemType;
        PriceYuanS = priceYuanS;
        Stack = stack;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        int yuanSItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>();
        int playerYuanS = player.CountItem(yuanSItemType);
        int totalCost = PriceYuanS * Stack;

        if (playerYuanS < totalCost)
        {
            Main.NewText($"元石不足！需要 {totalCost} 元石，你只有 {playerYuanS}。", Color.Red);
            return;
        }

        // 扣除元石
        ConsumeItemFromInventory(player, yuanSItemType, totalCost);
        // 给予物品
        player.QuickSpawnItem(new EntitySource_DebugCommand("BuyItem"), ItemType, Stack);
        Main.NewText($"购买成功！消耗了 {totalCost} 元石。", Color.Gold);
    }
}

/// <summary> 出售物品效果 - 出售物品获得元石 </summary>
public class SellItemEffect : DialogueEffect
{
    public int ItemType;
    public int PriceYuanS;
    public int MaxStack = 1;

    public SellItemEffect() { }

    public SellItemEffect(int itemType, int priceYuanS, int maxStack = 1)
    {
        ItemType = itemType;
        PriceYuanS = priceYuanS;
        MaxStack = maxStack;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        int playerCount = player.CountItem(ItemType);
        if (playerCount <= 0)
        {
            Main.NewText("你没有这个物品可以出售。", Color.Yellow);
            return;
        }

        int sellCount = System.Math.Min(playerCount, MaxStack);
        int totalIncome = PriceYuanS * sellCount;
        int yuanSItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>();

        // 扣除物品
        ConsumeItemFromInventory(player, ItemType, sellCount);
        // 给予元石
        player.QuickSpawnItem(new EntitySource_DebugCommand("SellItem"), yuanSItemType, totalIncome);
        Main.NewText($"出售成功！获得了 {totalIncome} 元石。", Color.Gold);
    }
}

/// <summary> 治疗效果 - 消耗元石恢复生命 </summary>
public class HealEffect : DialogueEffect
{
    public int HealAmount;
    public int CostYuanS;

    public HealEffect() { }

    public HealEffect(int healAmount, int costYuanS)
    {
        HealAmount = healAmount;
        CostYuanS = costYuanS;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        int yuanSItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>();
        int playerYuanS = player.CountItem(yuanSItemType);

        if (playerYuanS < CostYuanS)
        {
            Main.NewText($"元石不足！治疗需要 {CostYuanS} 元石。", Color.Red);
            return;
        }

        ConsumeItemFromInventory(player, yuanSItemType, CostYuanS);
        player.statLife = System.Math.Min(player.statLife + HealAmount, player.statLifeMax2);
        Main.NewText($"治疗完成！恢复了 {HealAmount} 点生命值。", Color.Green);
    }
}

/// <summary> 训练效果 - 提升玩家属性 </summary>
public class TrainEffect : DialogueEffect
{
    public enum TrainType { MaxLife, Damage, Defense }
    public TrainType Type;
    public int Amount;
    public int CostYuanS;

    public TrainEffect() { }

    public TrainEffect(TrainType type, int amount, int costYuanS)
    {
        Type = type;
        Amount = amount;
        CostYuanS = costYuanS;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        int yuanSItemType = ModContent.ItemType<Content.Items.Consumables.YuanS>();
        int playerYuanS = player.CountItem(yuanSItemType);

        if (playerYuanS < CostYuanS)
        {
            Main.NewText($"元石不足！训练需要 {CostYuanS} 元石。", Color.Red);
            return;
        }

        ConsumeItemFromInventory(player, yuanSItemType, CostYuanS);

        switch (Type)
        {
            case TrainType.MaxLife:
                player.statLifeMax2 += Amount;
                Main.NewText($"训练完成！最大生命值提升了 {Amount} 点。", Color.Green);
                break;
            case TrainType.Damage:
                // 使用通用伤害加成（通过 buff 模拟）
                player.AddBuff(BuffID.Wrath, 3600 * Amount); // 每点=1小时 wrath
                Main.NewText($"训练完成！获得了临时伤害加成。", Color.Green);
                break;
            case TrainType.Defense:
                player.AddBuff(BuffID.Endurance, 3600 * Amount);
                Main.NewText($"训练完成！获得了临时防御加成。", Color.Green);
                break;
        }
    }
}

/// <summary> 声望效果 - 修改与特定势力的声望 </summary>
public class ReputationEffect : DialogueEffect
{
    public FactionID Faction;
    public int Amount;
    public string Reason;

    public ReputationEffect() { }

    public ReputationEffect(FactionID faction, int amount, string reason = "")
    {
        Faction = faction;
        Amount = amount;
        Reason = reason;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        var worldPlayer = player.GetModPlayer<GuWorldPlayer>();
        if (Amount >= 0)
            worldPlayer.AddReputation(Faction, Amount, Reason);
        else
            worldPlayer.RemoveReputation(Faction, -Amount, Reason);
        string change = Amount >= 0 ? $"+{Amount}" : $"{Amount}";
        Main.NewText($"与 {WorldStateMachine.GetFactionDisplayName(Faction)} 的声望 {change}！", Color.LightBlue);
    }
}

/// <summary> 揭示地图效果 - 标记位置 </summary>
public class RevealMapEffect : DialogueEffect
{
    public string Label;
    public float WorldX;
    public float WorldY;

    public RevealMapEffect() { }

    public RevealMapEffect(string label, float worldX, float worldY)
    {
        Label = label;
        WorldX = worldX;
        WorldY = worldY;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        Main.NewText($"情报已记录：{Label} 位于 ({WorldX:F0}, {WorldY:F0})", Color.Cyan);
        // 实际地图标记功能需要 tModLayer 地图 API，这里先做文本提示
    }
}

/// <summary> 给予任务效果 - 触发任务/悬赏 </summary>
public class GiveQuestEffect : DialogueEffect
{
    public string QuestName;
    public string Description;
    public int RewardYuanS;

    public GiveQuestEffect() { }

    public GiveQuestEffect(string questName, string description, int rewardYuanS)
    {
        QuestName = questName;
        Description = description;
        RewardYuanS = rewardYuanS;
    }

    public override void Execute(Player player, NPC npc, BeliefState belief)
    {
        Main.NewText($"接受任务：{QuestName}", Color.Cyan);
        Main.NewText(Description, Color.White);
        Main.NewText($"奖励：{RewardYuanS} 元石", Color.Gold);
        // 任务系统预留 - 后续集成到 QuestSystem
    }
}
