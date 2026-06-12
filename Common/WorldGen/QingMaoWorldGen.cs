using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.GameContent.Generation;
using Terraria.WorldBuilding;

namespace VerminLordMod.Common.WorldGen
{
    public class QingMaoWorldGen : ModSystem
    {
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // 在地表生成之后插入青茅山地形
            int surfaceIndex = tasks.FindIndex(t => t.Name == "Surface");
            if (surfaceIndex != -1)
            {
                tasks.Insert(surfaceIndex + 1, new PassLegacy("VerminLord: QingMao Mountain", GenerateQingMaoMountain));
            }
        }

        private void GenerateQingMaoMountain(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "生成青茅山地形...";

            int worldCenter = Main.maxTilesX / 2;
            int mountainWidth = 200;
            int mountainHeight = 80;

            // 青茅山主体
            for (int x = worldCenter - mountainWidth / 2; x < worldCenter + mountainWidth / 2; x++)
            {
                float distanceFromCenter = Math.Abs(x - worldCenter) / (float)(mountainWidth / 2);
                float heightFactor = 1f - (float)Math.Pow(distanceFromCenter, 1.5);
                int surfaceY = (int)(Main.worldSurface - mountainHeight * heightFactor);

                for (int y = surfaceY; y < Main.maxTilesY; y++)
                {
                    if (y < Main.worldSurface + 5)
                    {
                        // 地表：青茅草 -> 使用草地方块
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = true;
                        tile.TileType = TileID.Grass;
                    }
                    else if (y < Main.worldSurface + 30)
                    {
                        // 浅层：青茅石 -> 使用石块
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = true;
                        tile.TileType = TileID.Stone;
                    }
                    else
                    {
                        // 深层：普通石块
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = true;
                        tile.TileType = TileID.Stone;
                    }
                }
            }

            // 古月山寨建筑（山腰处）
            int villageCenterX = worldCenter;
            int villageY = (int)(Main.worldSurface - mountainHeight * 0.5);

            // 议事厅
            PlaceBuilding(villageCenterX - 20, villageY, 40, 20,
                TileID.Stone, TileID.WoodBlock);

            // 训练场
            PlaceBuilding(villageCenterX + 30, villageY + 5, 25, 12,
                TileID.Stone, TileID.WoodBlock);

            // 药堂
            PlaceBuilding(villageCenterX - 55, villageY + 3, 20, 15,
                TileID.Stone, TileID.WoodBlock);

            progress.Value = 1f;
        }

        private void PlaceBuilding(int startX, int startY, int width, int height, ushort wallType, ushort floorType)
        {
            // 地板
            for (int x = startX; x < startX + width; x++)
            {
                for (int y = startY + height - 2; y < startY + height; y++)
                {
                    if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                    {
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = true;
                        tile.TileType = floorType;
                    }
                }
            }

            // 墙壁（左右两侧）
            for (int y = startY; y < startY + height; y++)
            {
                // 左墙
                if (startX >= 0 && startX < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                {
                    Tile tile = Main.tile[startX, y];
                    tile.HasTile = true;
                    tile.TileType = wallType;
                }
                // 右墙
                int rightX = startX + width - 1;
                if (rightX >= 0 && rightX < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                {
                    Tile tile = Main.tile[rightX, y];
                    tile.HasTile = true;
                    tile.TileType = wallType;
                }
            }

            // 清空内部
            for (int x = startX + 2; x < startX + width - 2; x++)
            {
                for (int y = startY + 2; y < startY + height - 2; y++)
                {
                    if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                    {
                        Tile tile = Main.tile[x, y];
                        tile.HasTile = false;
                    }
                }
            }
        }
    }
}
