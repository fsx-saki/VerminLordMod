using Microsoft.Xna.Framework;
using SubworldLibrary;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace VerminLordMod.Common.SubWorlds
{
    /// <summary>
    /// GuYueTerritory — 古月族地小世界（P2 MVA 阶段）
    /// 
    /// 职责：
    /// 1. 提供古月家族的驻地小世界
    /// 2. 包含青砖建筑、训练场、议事厅等预设结构
    /// 3. 进入时提示安全区规则（禁止公开战斗）
    /// 4. 进入时自动生成古月家族NPC，退出时保存NPC状态
    /// 
    /// MVA 阶段：
    /// - 平坦地形 + 预设建筑（青砖平台 + 墙壁）
    /// - 进入条件：声望 >= 中立（-20 以上）
    /// - 自动管理NPC生成和持久化
    /// 
    /// 依赖：
    /// - SubworldLibrary（第三方库，已引用）
    /// - GuWorldPlayer（声望检查）
    /// - GuYueTerritoryNPCSystem（NPC管理）
    /// </summary>
    public class GuYueTerritory : Subworld
    {
        // ===== 小世界尺寸 =====
        public override int Width => 400;
        public override int Height => 300;

        // ===== 持久化设置 =====
        public override bool ShouldSave => true;
        public override bool NoPlayerSaving => true;

        // ===== 世界生成任务 =====
        public override List<GenPass> Tasks => new List<GenPass>
        {
            new GuYueTerrainGenPass()
        };

        // ===== 生命周期 =====

        /// <summary> 进入小世界时 </summary>
        public override void OnLoad()
        {
            Main.dayTime = true;
            Main.time = 27000; // 正午
        }

        public override void OnEnter()
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText("进入古月族地。此处名义安全，禁止公开战斗。", Color.Cyan);
            }

            // 进入时生成NPC（如果尚未生成）
            GuYueTerritoryNPCSystem.SpawnAllNPCs();
        }

        public override void OnExit()
        {
            // 退出时保存NPC状态
            GuYueTerritoryNPCSystem.SaveCurrentNPCs();

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText("离开古月族地。", Color.Gray);
            }
        }
    }

    // ============================================================
    // 地形生成
    // ============================================================

    /// <summary>
    /// 古月族地地形生成器。
    /// MVA 阶段：平坦青砖地面 + 围墙 + 预设建筑轮廓。
    /// 
    /// 建筑布局（从左到右）：
    ///   杂役区(左) | 学堂(左中) | 训练场(左中) | 议事厅(中央) | 药堂(右中) | 御堂(右中) | 市场(右) | 居住区(右)
    /// </summary>
    public class GuYueTerrainGenPass : GenPass
    {
        public GuYueTerrainGenPass() : base("GuYueTerrain", 1) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "生成古月族地...";

            // 隐藏地下层和洞穴层
            Main.worldSurface = Main.maxTilesY - 10;
            Main.rockLayer = Main.maxTilesY;

            int width = Main.maxTilesX;
            int height = Main.maxTilesY;

            // 1. 填充地面层（青砖地面）
            int groundLevel = height / 2 + 20; // 地面 Y 坐标

            for (int i = 0; i < width; i++)
            {
                for (int j = groundLevel; j < height; j++)
                {
                    progress.Set((j + i * height) / (float)(width * height));

                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GrayBrick; // 青砖地面
                }
            }

            // 2. 建造围墙（边界）
            BuildWalls(width, height, groundLevel);

            // 3. 建造预设建筑（从左到右布局）
            int centerX = width / 2;

            // 左侧区域
            BuildServantQuarters(centerX - 120, groundLevel - 5);   // 杂役区（最左）
            BuildSchool(centerX - 75, groundLevel - 8);             // 学堂（左中）
            BuildTrainingGround(centerX - 40, groundLevel - 5);     // 训练场（左中偏中）

            // 中央区域
            BuildMainHall(centerX, groundLevel - 10);               // 议事厅（中央）

            // 右侧区域
            BuildMedicineHall(centerX + 40, groundLevel - 8);       // 药堂（右中）
            BuildDefenseHall(centerX + 75, groundLevel - 8);        // 御堂（右中偏右）
            BuildMarket(centerX + 110, groundLevel - 5);            // 市场（右）
            BuildResidence(centerX + 145, groundLevel - 5);         // 居住区（最右）

            // 4. 放置光源
            PlaceTorches(width, groundLevel);

            // 5. 铺设主路（连接各建筑的水平石板路）
            BuildMainRoad(width, groundLevel);
        }

        /// <summary>
        /// 建造围墙。
        /// </summary>
        private void BuildWalls(int width, int height, int groundLevel)
        {
            // 左右围墙
            for (int j = groundLevel - 15; j < height; j++)
            {
                // 左墙
                Tile leftWall = Main.tile[5, j];
                leftWall.HasTile = true;
                leftWall.TileType = TileID.GrayBrick;

                // 右墙
                Tile rightWall = Main.tile[width - 6, j];
                rightWall.HasTile = true;
                rightWall.TileType = TileID.GrayBrick;
            }

            // 大门（左右墙中间留空）
            for (int j = groundLevel - 5; j < groundLevel + 2; j++)
            {
                Tile leftDoor = Main.tile[5, j];
                leftDoor.HasTile = false; // 左门
                Tile rightDoor = Main.tile[width - 6, j];
                rightDoor.HasTile = false; // 右门
            }
        }

        /// <summary>
        /// 铺设主路（水平石板路连接各建筑）
        /// </summary>
        private void BuildMainRoad(int width, int groundLevel)
        {
            int roadY = groundLevel + 2;
            for (int i = 10; i < width - 10; i++)
            {
                for (int j = roadY; j <= roadY + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GoldBrick; // 金色石板路，突出主路
                }
            }
        }

        /// <summary>
        /// 建造议事厅（中央主建筑）。
        /// </summary>
        private void BuildMainHall(int centerX, int topY)
        {
            int halfWidth = 15;
            int height = 12;

            // 地板
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY + height; j <= topY + height + 2; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GoldBrick;
                }
            }

            // 墙壁
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY; j <= topY + height; j++)
                {
                    // 墙壁用灰砖
                    if (i == centerX - halfWidth || i == centerX + halfWidth || j == topY)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.GrayBrick;
                    }
                    // 内部空间
                    else if (j > topY && j < topY + height)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = false;
                        tile.WallType = WallID.Planked;
                    }
                }
            }

            // 大门
            int doorX = centerX;
            for (int j = topY + height - 3; j <= topY + height; j++)
            {
                Tile doorTile = Main.tile[doorX, j];
                doorTile.HasTile = false;
            }

            // 内部装饰：座椅（用木椅表示）
            Tile chair1 = Main.tile[centerX - 5, topY + height - 1];
            chair1.HasTile = true;
            chair1.TileType = TileID.Chairs;
            chair1.IsActuated = true;

            Tile chair2 = Main.tile[centerX + 5, topY + height - 1];
            chair2.HasTile = true;
            chair2.TileType = TileID.Chairs;
            chair2.IsActuated = true;

            // 内部装饰：桌子
            Tile table = Main.tile[centerX, topY + height - 1];
            table.HasTile = true;
            table.TileType = TileID.Tables;
        }

        /// <summary>
        /// 建造学堂（左侧）。
        /// </summary>
        private void BuildSchool(int centerX, int topY)
        {
            int halfWidth = 10;
            int height = 8;

            // 地板
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY + height; j <= topY + height + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GrayBrick;
                }
            }

            // 墙壁
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY; j <= topY + height; j++)
                {
                    if (i == centerX - halfWidth || i == centerX + halfWidth || j == topY)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.GrayBrick;
                    }
                    else if (j > topY && j < topY + height)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = false;
                        tile.WallType = WallID.Planked;
                    }
                }
            }

            // 大门
            Tile doorTile = Main.tile[centerX, topY + height - 2];
            doorTile.HasTile = false;
            Tile doorTile2 = Main.tile[centerX, topY + height - 1];
            doorTile2.HasTile = false;

            // 内部装饰：书架（用书柜表示）
            for (int k = -3; k <= 3; k += 2)
            {
                Tile bookShelf = Main.tile[centerX + k, topY + height - 3];
                bookShelf.HasTile = true;
                bookShelf.TileType = TileID.Bookcases;
            }

            // 内部装饰：讲台（用工作台表示）
            Tile podium = Main.tile[centerX, topY + height - 4];
            podium.HasTile = true;
            podium.TileType = TileID.WorkBenches;
        }

        /// <summary>
        /// 建造训练场。
        /// </summary>
        private void BuildTrainingGround(int centerX, int topY)
        {
            int halfWidth = 12;
            int height = 6;

            // 地板
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY + height; j <= topY + height + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GrayBrick;
                }
            }

            // 木人桩（用木柱表示）
            for (int k = -2; k <= 2; k++)
            {
                int x = centerX + k * 5;
                for (int j = topY + 2; j <= topY + height; j++)
                {
                    Tile tile = Main.tile[x, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.WoodenBeam;
                }
            }

            // 武器架（用木梁表示）
            Tile weaponRack = Main.tile[centerX - 8, topY + height - 2];
            weaponRack.HasTile = true;
            weaponRack.TileType = TileID.WoodenBeam;
        }

        /// <summary>
        /// 建造药堂（右侧偏内）。
        /// </summary>
        private void BuildMedicineHall(int centerX, int topY)
        {
            int halfWidth = 10;
            int height = 8;

            // 地板
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY + height; j <= topY + height + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GrayBrick;
                }
            }

            // 墙壁
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY; j <= topY + height; j++)
                {
                    if (i == centerX - halfWidth || i == centerX + halfWidth || j == topY)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.GrayBrick;
                    }
                    else if (j > topY && j < topY + height)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = false;
                        tile.WallType = WallID.Planked;
                    }
                }
            }

            // 大门
            Tile doorTile = Main.tile[centerX, topY + height - 2];
            doorTile.HasTile = false;
            Tile doorTile2 = Main.tile[centerX, topY + height - 1];
            doorTile2.HasTile = false;

            // 内部装饰：药柜（用箱子表示）
            for (int k = -3; k <= 3; k += 2)
            {
                Tile chest = Main.tile[centerX + k, topY + height - 3];
                chest.HasTile = true;
                chest.TileType = TileID.Containers;
            }

            // 内部装饰：炼药台（用炼药台表示）
            Tile alchemy = Main.tile[centerX, topY + height - 4];
            alchemy.HasTile = true;
            alchemy.TileType = TileID.AlchemyTable;
        }

        /// <summary>
        /// 建造御堂（防御堂，右侧）。
        /// </summary>
        private void BuildDefenseHall(int centerX, int topY)
        {
            int halfWidth = 10;
            int height = 8;

            // 地板
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY + height; j <= topY + height + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GrayBrick;
                }
            }

            // 墙壁
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY; j <= topY + height; j++)
                {
                    if (i == centerX - halfWidth || i == centerX + halfWidth || j == topY)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.GrayBrick;
                    }
                    else if (j > topY && j < topY + height)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = false;
                        tile.WallType = WallID.Planked;
                    }
                }
            }

            // 大门
            Tile doorTile = Main.tile[centerX, topY + height - 2];
            doorTile.HasTile = false;
            Tile doorTile2 = Main.tile[centerX, topY + height - 1];
            doorTile2.HasTile = false;

            // 内部装饰：武器架
            Tile weaponRack1 = Main.tile[centerX - 4, topY + height - 3];
            weaponRack1.HasTile = true;
            weaponRack1.TileType = TileID.WoodenBeam;

            Tile weaponRack2 = Main.tile[centerX + 4, topY + height - 3];
            weaponRack2.HasTile = true;
            weaponRack2.TileType = TileID.WoodenBeam;
        }

        /// <summary>
        /// 建造市场（右侧）。
        /// </summary>
        private void BuildMarket(int centerX, int topY)
        {
            int halfWidth = 12;
            int height = 6;

            // 地板（开放市场，无墙壁）
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY + height; j <= topY + height + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GoldBrick; // 金色地板突出市场
                }
            }

            // 摊位（用桌子表示）
            for (int k = -4; k <= 4; k += 2)
            {
                int x = centerX + k * 3;
                for (int j = topY + 2; j <= topY + height - 1; j++)
                {
                    // 摊位柱子
                    if (j == topY + 2 || j == topY + height - 1)
                    {
                        Tile pillar = Main.tile[x, j];
                        pillar.HasTile = true;
                        pillar.TileType = TileID.WoodenBeam;
                    }
                }
                // 摊位顶棚
                Tile roof = Main.tile[x, topY + 1];
                roof.HasTile = true;
                roof.TileType = TileID.WoodBlock;

                // 摊位桌面
                Tile table = Main.tile[x, topY + height - 1];
                table.HasTile = true;
                table.TileType = TileID.Tables;
            }
        }

        /// <summary>
        /// 建造杂役区（最左侧）。
        /// </summary>
        private void BuildServantQuarters(int centerX, int topY)
        {
            int halfWidth = 12;
            int height = 6;

            // 地板
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY + height; j <= topY + height + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GrayBrick;
                }
            }

            // 简易墙壁（只有后墙和侧墙，前面开放）
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY; j <= topY + height; j++)
                {
                    if (i == centerX - halfWidth || i == centerX + halfWidth || j == topY)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.GrayBrick;
                    }
                    else if (j > topY && j < topY + height)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = false;
                        tile.WallType = WallID.Wood;
                    }
                }
            }

            // 内部：工具架（用工作台表示）
            Tile workbench = Main.tile[centerX - 3, topY + height - 2];
            workbench.HasTile = true;
            workbench.TileType = TileID.WorkBenches;

            Tile workbench2 = Main.tile[centerX + 3, topY + height - 2];
            workbench2.HasTile = true;
            workbench2.TileType = TileID.WorkBenches;

            // 内部：储物箱
            Tile chest = Main.tile[centerX, topY + height - 3];
            chest.HasTile = true;
            chest.TileType = TileID.Containers;
        }

        /// <summary>
        /// 建造居住区（最右侧）。
        /// </summary>
        private void BuildResidence(int centerX, int topY)
        {
            int halfWidth = 10;
            int height = 8;

            // 地板
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY + height; j <= topY + height + 1; j++)
                {
                    Tile tile = Main.tile[i, j];
                    tile.HasTile = true;
                    tile.TileType = TileID.GrayBrick;
                }
            }

            // 墙壁
            for (int i = centerX - halfWidth; i <= centerX + halfWidth; i++)
            {
                for (int j = topY; j <= topY + height; j++)
                {
                    if (i == centerX - halfWidth || i == centerX + halfWidth || j == topY)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = true;
                        tile.TileType = TileID.GrayBrick;
                    }
                    else if (j > topY && j < topY + height)
                    {
                        Tile tile = Main.tile[i, j];
                        tile.HasTile = false;
                        tile.WallType = WallID.Planked;
                    }
                }
            }

            // 小门
            Tile smallDoor1 = Main.tile[centerX, topY + height - 2];
            smallDoor1.HasTile = false;
            Tile smallDoor2 = Main.tile[centerX, topY + height - 1];
            smallDoor2.HasTile = false;

            // 内部装饰：床
            Tile bed = Main.tile[centerX - 3, topY + height - 3];
            bed.HasTile = true;
            bed.TileType = TileID.Beds;

            // 内部装饰：桌子
            Tile table = Main.tile[centerX + 3, topY + height - 3];
            table.HasTile = true;
            table.TileType = TileID.Tables;
        }

        /// <summary>
        /// 放置火把光源。
        /// </summary>
        private void PlaceTorches(int width, int groundLevel)
        {
            // 沿主路每隔一段距离放置火把
            for (int i = 20; i < width - 20; i += 30)
            {
                int x = i;
                int y = groundLevel - 1;
                if (x > 0 && x < width && y > 0 && y < Main.maxTilesY)
                {
                    Tile tile = Main.tile[x, y];
                    tile.HasTile = true;
                    tile.TileType = TileID.Torches;
                }
            }

            // 建筑门口额外放置火把
            int centerX = width / 2;
            int groundLevel2 = Main.maxTilesY / 2 + 20;

            // 议事厅门口
            PlaceTorchAt(centerX, groundLevel2 - 12);
            // 学堂门口
            PlaceTorchAt(centerX - 75, groundLevel2 - 10);
            // 药堂门口
            PlaceTorchAt(centerX + 40, groundLevel2 - 10);
            // 御堂门口
            PlaceTorchAt(centerX + 75, groundLevel2 - 10);
        }

        private void PlaceTorchAt(int x, int y)
        {
            if (x > 0 && x < Main.maxTilesX && y > 0 && y < Main.maxTilesY)
            {
                Tile tile = Main.tile[x, y];
                tile.HasTile = true;
                tile.TileType = TileID.Torches;
            }
        }
    }

    // ============================================================
    // 小世界更新系统
    // ============================================================

    /// <summary>
    /// 古月族地小世界的帧更新。
    /// 确保小世界内的机制正常运行（电路、液体、Tile实体）。
    /// 同时管理NPC的生成和持久化。
    /// </summary>
    public class GuYueTerritoryUpdateSystem : ModSystem
    {
        public override void PreUpdateWorld()
        {
            if (SubworldSystem.IsActive<GuYueTerritory>())
            {
                // 更新电路
                Wiring.UpdateMech();

                // 更新 Tile 实体
                TileEntity.UpdateStart();
                foreach (TileEntity te in TileEntity.ByID.Values)
                {
                    te.Update();
                }
                TileEntity.UpdateEnd();

                // 更新液体
                if (++Liquid.skipCount > 1)
                {
                    Liquid.UpdateLiquid();
                    Liquid.skipCount = 0;
                }

                // 确保NPC已生成（在OnEnter之后，如果世界是从存档加载的）
                if (!GuYueTerritoryNPCSystem.HasInitialized)
                {
                    GuYueTerritoryNPCSystem.SpawnAllNPCs();
                }
            }
        }
    }
}
