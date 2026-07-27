using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.QuestSystem
{
    /// <summary>
    /// QuestLog 已弃用，L 键不再打开任务界面。
    /// 保留 Keybind 注册以防其他系统引用，但不做任何操作。
    /// </summary>
    public class QuestKeybindSystem : ModSystem
    {
        public static ModKeybind QuestLogKey { get; private set; }

        public override void Load()
        {
            QuestLogKey = KeybindLoader.RegisterKeybind(Mod, "QuestLog", "L");
        }

        public override void Unload()
        {
            QuestLogKey = null;
        }

        public override void PostUpdateInput()
        {
            // QuestLog 已移除，L 键无操作
        }
    }
}
