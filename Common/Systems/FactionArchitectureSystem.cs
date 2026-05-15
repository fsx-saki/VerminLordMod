using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace VerminLordMod.Common.Systems
{
    public enum FactionArchitectureStyle
    {
        None = 0,
        GuYueStyle,         // 古月 — 青砖/竹木/红铜，南方山寨风
        BaiStyle,           // 白家 — 白玉/银饰，清雅世家风
        XiongStyle,         // 熊家 — 粗石/铁骨，厚重堡垒风
        TieStyle,           // 铁家 — 黑铁/熔炉，锻造工坊风
        Bai2Style,          // 百家 — 混搭/实用，市井百业风
        WangStyle,          // 汪家 — 水晶/流水，水乡泽国风
        ZhaoStyle,          // 赵家 — 暗石/机关，密道暗城风
        JiaStyle,           // 贾家 — 金饰/丝绸，商贾奢华风
        ScatteredStyle,     // 散修 — 简陋/自然，野居洞穴风
    }

    public enum StructureType
    {
        Wall,               // 墙壁
        Floor,              // 地板
        Platform,           // 平台
        Furniture,          // 家具
        Door,               // 门
        LightSource,        // 光源
        Decoration,         // 装饰
        Functional,         // 功能建筑（工作台/炼丹炉等）
        Special,            // 特殊（祭坛/阵法等）
    }

    public class TilePlacement
    {
        public int X;
        public int Y;
        public ushort TileID;
        public short FrameX;
        public short FrameY;
        public int WallID;
        public bool IsWall;
        public bool IsPlatform;
        public StructureType Type;
    }

    public class StructureTemplate
    {
        public string TemplateID;
        public string DisplayName;
        public FactionArchitectureStyle Style;
        public int Width;
        public int Height;
        public List<TilePlacement> Tiles = new();
        public List<string> RequiredTileTypes = new();
    }

    public class FactionArchitectureSystem : ModSystem
    {
        public static FactionArchitectureSystem Instance => ModContent.GetInstance<FactionArchitectureSystem>();

        public Dictionary<string, StructureTemplate> Templates = new();
        public Dictionary<FactionArchitectureStyle, List<string>> StyleTemplates = new();

        public override void OnWorldLoad()
        {
            Templates.Clear();
            StyleTemplates.Clear();
            RegisterDefaultTemplates();
        }

        private void RegisterDefaultTemplates()
        {
            // TODO: 注册各家族默认建筑模板
            RegisterGuYueTemplates();
            RegisterBaiTemplates();
            RegisterXiongTemplates();
        }

        private void RegisterGuYueTemplates()
        {
            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "guyue_stilted_house",
                DisplayName = "古月吊脚楼",
                Style = FactionArchitectureStyle.GuYueStyle,
                Width = 12,
                Height = 10,
            });

            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "guyue_ancestral_hall",
                DisplayName = "古月祠堂",
                Style = FactionArchitectureStyle.GuYueStyle,
                Width = 20,
                Height = 14,
            });

            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "guyue_school",
                DisplayName = "古月学堂",
                Style = FactionArchitectureStyle.GuYueStyle,
                Width = 16,
                Height = 10,
            });
        }

        private void RegisterBaiTemplates()
        {
            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "bai_manor",
                DisplayName = "白家府邸",
                Style = FactionArchitectureStyle.BaiStyle,
                Width = 24,
                Height = 16,
            });
        }

        private void RegisterXiongTemplates()
        {
            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "xiong_fortress",
                DisplayName = "熊家堡垒",
                Style = FactionArchitectureStyle.XiongStyle,
                Width = 20,
                Height = 18,
            });
        }

        private void RegisterTemplate(StructureTemplate template)
        {
            Templates[template.TemplateID] = template;
            if (!StyleTemplates.ContainsKey(template.Style))
                StyleTemplates[template.Style] = new List<string>();
            StyleTemplates[template.Style].Add(template.TemplateID);
        }

        public static FactionArchitectureStyle GetFactionStyle(FactionID faction)
        {
            return faction switch
            {
                FactionID.GuYue => FactionArchitectureStyle.GuYueStyle,
                FactionID.Bai => FactionArchitectureStyle.BaiStyle,
                FactionID.Xiong => FactionArchitectureStyle.XiongStyle,
                FactionID.Tie => FactionArchitectureStyle.TieStyle,
                FactionID.Bai2 => FactionArchitectureStyle.Bai2Style,
                FactionID.Wang => FactionArchitectureStyle.WangStyle,
                FactionID.Zhao => FactionArchitectureStyle.ZhaoStyle,
                FactionID.Jia => FactionArchitectureStyle.JiaStyle,
                FactionID.Scattered => FactionArchitectureStyle.ScatteredStyle,
                _ => FactionArchitectureStyle.None,
            };
        }

        public static int GetFactionWallTile(FactionID faction)
        {
            // TODO: 返回各家族对应的墙壁Tile ID
            return faction switch
            {
                FactionID.GuYue => TileID.BorealWood,
                _ => TileID.Stone,
            };
        }

        public static int GetFactionFloorTile(FactionID faction)
        {
            // TODO: 返回各家族对应的地板Tile ID
            return faction switch
            {
                FactionID.GuYue => TileID.BorealWood,
                _ => TileID.StoneSlab,
            };
        }

        public void PlaceStructure(string templateID, int startX, int startY)
        {
            if (!Templates.TryGetValue(templateID, out var template)) return;

            foreach (var tile in template.Tiles)
            {
                int x = startX + tile.X;
                int y = startY + tile.Y;

                if (!WorldGen.InWorld(x, y)) continue;

                if (tile.IsWall)
                {
                    WorldGen.PlaceWall(x, y, tile.WallID);
                }
                else
                {
                    WorldGen.PlaceTile(x, y, tile.TileID);
                    if (Main.tile[x, y].HasTile)
                    {
                        Main.tile[x, y].TileFrameX = tile.FrameX;
                        Main.tile[x, y].TileFrameY = tile.FrameY;
                    }
                }
            }
        }

        public StructureTemplate LoadTemplateFromText(string text)
        {
            var template = new StructureTemplate();
            var reader = new StringReader(text);
            string line;
            int y = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                {
                    ParseTemplateHeader(line, template);
                    continue;
                }

                if (line.StartsWith("@"))
                {
                    ParseTemplateMeta(line, template);
                    continue;
                }

                for (int x = 0; x < line.Length; x++)
                {
                    char c = line[x];
                    if (c == '.' || c == ' ') continue;

                    var placement = CharToTilePlacement(c, x, y);
                    if (placement != null)
                        template.Tiles.Add(placement);
                }
                y++;
            }

            template.Height = y;
            return template;
        }

        private void ParseTemplateHeader(string line, StructureTemplate template)
        {
            if (line.StartsWith("#ID:"))
                template.TemplateID = line.Substring(4).Trim();
            else if (line.StartsWith("#NAME:"))
                template.DisplayName = line.Substring(6).Trim();
            else if (line.StartsWith("#STYLE:"))
                System.Enum.TryParse(line.Substring(7).Trim(), out template.Style);
        }

        private void ParseTemplateMeta(string line, StructureTemplate template)
        {
            // @TILE:A=123 格式：字符A映射到TileID 123
            if (line.StartsWith("@TILE:"))
            {
                var parts = line.Substring(6).Split('=');
                if (parts.Length == 2 && char.TryParse(parts[0], out char c) && int.TryParse(parts[1], out int id))
                {
                    // TODO: 存储字符到TileID的映射
                }
            }
        }

        private TilePlacement CharToTilePlacement(char c, int x, int y)
        {
            // TODO: 完善字符到Tile的映射
            return c switch
            {
                'W' => new TilePlacement { X = x, Y = y, IsWall = true, WallID = WallID.BorealWood, Type = StructureType.Wall },
                'F' => new TilePlacement { X = x, Y = y, TileID = TileID.BorealWood, Type = StructureType.Floor },
                'P' => new TilePlacement { X = x, Y = y, TileID = TileID.Platforms, IsPlatform = true, Type = StructureType.Platform },
                'D' => new TilePlacement { X = x, Y = y, TileID = TileID.ClosedDoor, Type = StructureType.Door },
                'T' => new TilePlacement { X = x, Y = y, TileID = TileID.Tables, Type = StructureType.Furniture },
                'C' => new TilePlacement { X = x, Y = y, TileID = TileID.Chairs, Type = StructureType.Furniture },
                'L' => new TilePlacement { X = x, Y = y, TileID = TileID.Torches, Type = StructureType.LightSource },
                _ => null,
            };
        }

        public override void SaveWorldData(TagCompound tag)
        {
            // TODO: 保存建筑模板缓存
        }

        public override void LoadWorldData(TagCompound tag)
        {
            // TODO: 加载建筑模板缓存
        }
    }
}
