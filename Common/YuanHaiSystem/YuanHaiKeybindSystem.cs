using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.YuanHaiSystem
{
    public class YuanHaiKeybindSystem : ModSystem
    {
        public static ModKeybind YuanHaiKey { get; private set; }
        public static ModKeybind RefiningKey { get; private set; }

        public override void Load()
        {
            YuanHaiKey = KeybindLoader.RegisterKeybind(Mod, "YuanHai", "Y");
            RefiningKey = KeybindLoader.RegisterKeybind(Mod, "Refining", "U");
        }

        public override void Unload()
        {
            YuanHaiKey = null;
            RefiningKey = null;
        }

        public override void PostUpdateInput()
        {
            if (Main.netMode == NetmodeID.Server) return;

            try
            {
                if (YuanHaiKey?.JustPressed == true)
                {
                    var ui = InnoVault.UIHandles.UIHandleLoader.GetUIHandleOfType<YuanHaiUI>();
                    ui.Toggle();
                }
                if (RefiningKey?.JustPressed == true)
                {
                    var ui = InnoVault.UIHandles.UIHandleLoader.GetUIHandleOfType<RefiningUI>();
                    ui.Toggle();
                }
            }
            catch { }
        }
    }
}
