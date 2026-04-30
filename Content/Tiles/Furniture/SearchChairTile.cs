// ============================================================
// SearchChairTile - 搜索椅 Tile
// 玩家靠近放置的椅子时，显示信息提示框（"搜索"按钮）
// 点击搜索按钮关闭信息框，弹出面板播放搜索动画
// ============================================================
#nullable enable
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using VerminLordMod.Common.UI.SimpleUI;
using VerminLordMod.Content.Items.Debuggers.SearchChair;

namespace VerminLordMod.Content.Tiles.Furniture;

/// <summary>
/// 搜索椅 Tile — 玩家靠近时触发搜索 UI
/// </summary>
public class SearchChairTile : ModTile
{
    public const int NextStyleHeight = 40;

    public override void SetStaticDefaults()
    {
        // Properties
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        TileID.Sets.HasOutlines[Type] = true;
        TileID.Sets.CanBeSatOnForNPCs[Type] = true;
        TileID.Sets.CanBeSatOnForPlayers[Type] = true;
        TileID.Sets.DisableSmartCursor[Type] = true;

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);

        AdjTiles = new int[] { TileID.Chairs };

        // Names
        AddMapEntry(new Color(200, 200, 200), Language.GetText("MapObject.Chair"));

        // Placement
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.CoordinateHeights = new[] { 16, 18 };
        TileObjectData.newTile.CoordinatePaddingFix = new Point16(0, 2);
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.newTile.StyleMultiplier = 2;
        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);
    }

    public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info)
    {
        Tile tile = Framing.GetTileSafely(i, j);

        info.TargetDirection = -1;
        if (tile.TileFrameX != 0)
        {
            info.TargetDirection = 1;
        }

        info.AnchorTilePosition.X = i;
        info.AnchorTilePosition.Y = j;

        if (tile.TileFrameY % NextStyleHeight == 0)
        {
            info.AnchorTilePosition.Y++;
        }
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;

        if (player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
        {
            player.GamepadEnableGrappleCooldown();
            player.sitting.SitDown(player, i, j);
        }

        return true;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;

        if (!player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance))
        {
            return;
        }

        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<Items.Debuggers.SearchChair.SearchChairItem>();

        if (Main.tile[i, j].TileFrameX / 18 < 1)
        {
            player.cursorItemIconReversed = true;
        }

        // 计算 Tile 在屏幕上的位置（用于 UI 定位在椅子上方）
        // Tile 坐标 → 世界坐标 → 屏幕坐标
        float worldX = i * 16f;
        float worldY = j * 16f;
        Vector2 tileScreenPos = new Vector2(
            worldX - Main.screenPosition.X,
            worldY - Main.screenPosition.Y
        );

        // 玩家靠近椅子时，触发搜索 UI，传递 Tile 屏幕位置
        SearchChairHandler.OnPlayerNearChair(tileScreenPos);
    }
}
