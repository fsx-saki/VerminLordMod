using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using VerminLordMod.Common.Systems;

namespace VerminLordMod.Content.Tiles.GuYueArchitecture
{
    public class MiZongZhenTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;

            DustType = DustID.PurpleTorch;

            AddMapEntry(new Color(100, 50, 150), Language.GetText("迷踪阵眼"));
        }

        public override bool RightClick(int i, int j)
        {
            var player = Main.LocalPlayer;
            DefenseSystem.Instance.RegisterMiZong(new Point(i, j), player.whoAmI);
            Main.NewText("迷踪阵已激活！", Color.Cyan);
            return true;
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            DefenseSystem.Instance.UnregisterMiZong(new Point(i, j));
        }
    }
}