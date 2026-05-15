using Microsoft.Xna.Framework;
using Terraria;

namespace VerminLordMod.Common.Abstractions
{
    public enum InteractionResultType
    {
        None = 0,
        Success,            // 交互成功
        Failed,             // 交互失败（条件不满足）
        NotInRange,         // 不在交互范围内
        OnCooldown,         // 冷却中
        TriggerCombat,      // 触发战斗
        OpenShop,           // 打开商店
        OpenDialogue,       // 打开对话
        Custom,             // 自定义结果
    }

    public interface IInteractable
    {
        string InteractableID { get; }
        float InteractionRange { get; }
        bool CanInteract(Player player);
        InteractionResultType Interact(Player player);
        string GetInteractionPrompt(Player player);
    }

    public interface IInteractableWithCooldown : IInteractable
    {
        int CooldownTicks { get; }
        int RemainingCooldown { get; }
        bool IsOnCooldown => RemainingCooldown > 0;
        void TickCooldown();
    }

    public interface IMultiInteractable : IInteractable
    {
        int InteractionCount { get; }
        string[] GetAvailableInteractions(Player player);
        InteractionResultType ExecuteInteraction(Player player, int interactionIndex);
    }

    public struct InteractionResult
    {
        public InteractionResultType Type;
        public string Message;
        public int CooldownTicks;
        public object CustomData;

        public static InteractionResult Success(string message = "")
            => new() { Type = InteractionResultType.Success, Message = message };

        public static InteractionResult Failed(string message = "")
            => new() { Type = InteractionResultType.Failed, Message = message };

        public static InteractionResult NotInRange()
            => new() { Type = InteractionResultType.NotInRange, Message = "距离太远" };

        public static InteractionResult OnCooldown(int remaining)
            => new() { Type = InteractionResultType.OnCooldown, Message = $"冷却中({remaining}帧)", CooldownTicks = remaining };

        public static InteractionResult Combat(string message = "")
            => new() { Type = InteractionResultType.TriggerCombat, Message = message };

        public static InteractionResult Shop(string message = "")
            => new() { Type = InteractionResultType.OpenShop, Message = message };

        public static InteractionResult Dialogue(string message = "")
            => new() { Type = InteractionResultType.OpenDialogue, Message = message };
    }

    public static class InteractableHelper
    {
        public static bool IsInRange(Vector2 entityPos, Vector2 playerPos, float range)
        {
            return Vector2.Distance(entityPos, playerPos) <= range;
        }

        public static float GetDistancePercent(Vector2 entityPos, Vector2 playerPos, float maxRange)
        {
            float dist = Vector2.Distance(entityPos, playerPos);
            return 1f - MathHelper.Clamp(dist / maxRange, 0f, 1f);
        }
    }
}
