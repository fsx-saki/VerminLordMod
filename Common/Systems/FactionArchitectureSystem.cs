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
            RegisterGuYueTemplates();
            RegisterBaiTemplates();
            RegisterXiongTemplates();
            RegisterTieTemplates();
            RegisterWangTemplates();
            RegisterZhaoTemplates();
            RegisterJiaTemplates();
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

            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "xiong_barracks",
                DisplayName = "熊家兵营",
                Style = FactionArchitectureStyle.XiongStyle,
                Width = 16,
                Height = 10,
            });
        }

        private void RegisterTieTemplates()
        {
            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "tie_forge",
                DisplayName = "铁家锻造坊",
                Style = FactionArchitectureStyle.TieStyle,
                Width = 14,
                Height = 10,
            });

            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "tie_armory",
                DisplayName = "铁家武库",
                Style = FactionArchitectureStyle.TieStyle,
                Width = 12,
                Height = 8,
            });
        }

        private void RegisterWangTemplates()
        {
            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "wang_pavilion",
                DisplayName = "汪家水榭",
                Style = FactionArchitectureStyle.WangStyle,
                Width = 16,
                Height = 12,
            });

            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "wang_library",
                DisplayName = "汪家藏书阁",
                Style = FactionArchitectureStyle.WangStyle,
                Width = 14,
                Height = 14,
            });
        }

        private void RegisterZhaoTemplates()
        {
            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "zhao_underground_hall",
                DisplayName = "赵家地下殿堂",
                Style = FactionArchitectureStyle.ZhaoStyle,
                Width = 18,
                Height = 12,
            });

            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "zhao_trap_chamber",
                DisplayName = "赵家机关室",
                Style = FactionArchitectureStyle.ZhaoStyle,
                Width = 10,
                Height = 8,
            });
        }

        private void RegisterJiaTemplates()
        {
            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "jia_trade_hall",
                DisplayName = "贾家商会大厅",
                Style = FactionArchitectureStyle.JiaStyle,
                Width = 22,
                Height = 14,
            });

            RegisterTemplate(new StructureTemplate
            {
                TemplateID = "jia_warehouse",
                DisplayName = "贾家仓库",
                Style = FactionArchitectureStyle.JiaStyle,
                Width = 16,
                Height = 10,
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
            return faction switch
            {
                FactionID.GuYue => WallID.BorealWood,
                FactionID.Bai => WallID.Marble,
                FactionID.Xiong => WallID.Granite,
                FactionID.Tie => WallID.ObsidianBrick,
                FactionID.Bai2 => WallID.Wood,
                FactionID.Wang => WallID.Glass,
                FactionID.Zhao => WallID.EbonstoneUnsafe,
                FactionID.Jia => WallID.GoldBrick,
                FactionID.Scattered => WallID.MudUnsafe,
                _ => WallID.Stone,
            };
        }

        public static int GetFactionFloorTile(FactionID faction)
        {
            return faction switch
            {
                FactionID.GuYue => TileID.BorealWood,
                FactionID.Bai => TileID.Marble,
                FactionID.Xiong => TileID.Granite,
                FactionID.Tie => TileID.ObsidianBrick,
                FactionID.Bai2 => TileID.WoodBlock,
                FactionID.Wang => TileID.Glass,
                FactionID.Zhao => TileID.EbonstoneBrick,
                FactionID.Jia => TileID.GoldBrick,
                FactionID.Scattered => TileID.Mudstone,
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

        private Dictionary<char, TilePlacement> _charMapping = new();

        private void ParseTemplateMeta(string line, StructureTemplate template)
        {
            if (line.StartsWith("@TILE:"))
            {
                var parts = line.Substring(6).Split('=');
                if (parts.Length == 2 && char.TryParse(parts[0], out char c) && int.TryParse(parts[1], out int id))
                {
                    _charMapping[c] = new TilePlacement { TileID = (ushort)id, Type = StructureType.Floor };
                }
            }
            else if (line.StartsWith("@WALL:"))
            {
                var parts = line.Substring(6).Split('=');
                if (parts.Length == 2 && char.TryParse(parts[0], out char c) && int.TryParse(parts[1], out int id))
                {
                    _charMapping[c] = new TilePlacement { IsWall = true, WallID = id, Type = StructureType.Wall };
                }
            }
        }

        private TilePlacement CharToTilePlacement(char c, int x, int y)
        {
            if (_charMapping.TryGetValue(c, out var mapping))
            {
                return new TilePlacement
                {
                    X = x, Y = y,
                    TileID = mapping.TileID,
                    WallID = mapping.WallID,
                    IsWall = mapping.IsWall,
                    IsPlatform = mapping.IsPlatform,
                    Type = mapping.Type,
                };
            }

            return c switch
            {
                'W' => new TilePlacement { X = x, Y = y, IsWall = true, WallID = WallID.BorealWood, Type = StructureType.Wall },
                'F' => new TilePlacement { X = x, Y = y, TileID = TileID.BorealWood, Type = StructureType.Floor },
                'P' => new TilePlacement { X = x, Y = y, TileID = TileID.Platforms, IsPlatform = true, Type = StructureType.Platform },
                'D' => new TilePlacement { X = x, Y = y, TileID = TileID.ClosedDoor, Type = StructureType.Door },
                'T' => new TilePlacement { X = x, Y = y, TileID = TileID.Tables, Type = StructureType.Furniture },
                'C' => new TilePlacement { X = x, Y = y, TileID = TileID.Chairs, Type = StructureType.Furniture },
                'L' => new TilePlacement { X = x, Y = y, TileID = TileID.Torches, Type = StructureType.LightSource },
                'B' => new TilePlacement { X = x, Y = y, TileID = TileID.Beds, Type = StructureType.Furniture },
                'K' => new TilePlacement { X = x, Y = y, TileID = TileID.Bookcases, Type = StructureType.Furniture },
                'S' => new TilePlacement { X = x, Y = y, TileID = TileID.Sinks, Type = StructureType.Furniture },
                'A' => new TilePlacement { X = x, Y = y, TileID = TileID.Anvils, Type = StructureType.Functional },
                'H' => new TilePlacement { X = x, Y = y, TileID = TileID.CookingPots, Type = StructureType.Functional },
                'O' => new TilePlacement { X = x, Y = y, TileID = TileID.Bottles, Type = StructureType.Functional },
                'X' => new TilePlacement { X = x, Y = y, TileID = TileID.DemonAltar, Type = StructureType.Special },
                _ => null,
            };
        }

        public override void SaveWorldData(TagCompound tag)
        {
            var templateList = new List<TagCompound>();
            foreach (var kvp in Templates)
            {
                templateList.Add(new TagCompound
                {
                    ["id"] = kvp.Key,
                    ["name"] = kvp.Value.DisplayName,
                    ["style"] = (int)kvp.Value.Style,
                    ["width"] = kvp.Value.Width,
                    ["height"] = kvp.Value.Height,
                });
            }
            tag["templates"] = templateList;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            Templates.Clear();
            StyleTemplates.Clear();

            var templateList = tag.GetList<TagCompound>("templates");
            if (templateList == null) return;

            foreach (var t in templateList)
            {
                var template = new StructureTemplate
                {
                    TemplateID = t.GetString("id"),
                    DisplayName = t.GetString("name"),
                    Style = (FactionArchitectureStyle)t.GetInt("style"),
                    Width = t.GetInt("width"),
                    Height = t.GetInt("height"),
                };
                RegisterTemplate(template);
            }
        }
    }
}
