using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.QuestSystem
{
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
            if (QuestLogKey?.JustPressed == true && Main.netMode != NetmodeID.Server)
            {
                var log = InnoVault.UIHandles.UIHandleLoader.GetUIHandleOfType<QuestLog>();
                log.visible = !log.visible;
                Terraria.Audio.SoundEngine.PlaySound(log.visible ? Terraria.ID.SoundID.MenuOpen : Terraria.ID.SoundID.MenuClose);
            }
        }
    }
}
