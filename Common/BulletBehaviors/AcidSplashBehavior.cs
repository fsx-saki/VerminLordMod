using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 酸液飞溅行为 — 弹幕销毁时在周围物块上留下酸液水渍。
    /// 
    /// 效果：
    /// 1. 在 projectile 周围 3x3 范围内检测可喷涂物块
    /// 2. 给物块涂上绿色/酸液颜色（使用 WorldGen.paintTile）
    /// 3. 在物块位置生成绿色 Dust 模拟酸液滴落
    /// 4. 可选：在物块上添加少量液体（Liquid.AddWater）
    /// 
    /// 注意：此行为只影响物块外观，不修改物块类型。
    /// 如需真正的物块转化（如石头→酸性石头），需要额外实现。
    /// </summary>
    public class AcidSplashBehavior : IBulletBehavior
    {
        public string Name => "AcidSplash";

        // ===== 可配置参数 =====

        /// <summary>检测半径（物块格数）</summary>
        public int Radius { get; set; } = 2;

        /// <summary>喷涂概率（0~1）</summary>
        public float PaintChance { get; set; } = 0.6f;

        /// <summary>喷涂颜色索引（对应 Terraria 的 PaintID）</summary>
        /// <remarks>常用：1=红, 2=橙, 3=黄, 4=青柠, 5=绿, 6=青, 7=浅蓝, 8=蓝, 9=紫, 10=粉</remarks>
        public byte PaintColor { get; set; } = 5; // 绿色

        /// <summary>是否添加液体（酸液效果）</summary>
        public bool AddLiquid { get; set; } = true;

        /// <summary>液体类型（0=水, 1=熔岩, 2=蜂蜜）</summary>
        public byte LiquidType { get; set; } = 0;

        /// <summary>液体添加量（0~255）</summary>
        public byte LiquidAmount { get; set; } = 32;

        /// <summary>是否生成 Dust 粒子</summary>
        public bool SpawnDust { get; set; } = true;

        /// <summary>Dust 数量</summary>
        public int DustCount { get; set; } = 8;

        /// <summary>Dust 类型（-1 则使用默认绿色 Dust）</summary>
        public int DustType { get; set; } = -1;

        /// <summary>是否只喷涂可喷涂物块（非家具、非墙壁）</summary>
        public bool OnlyPaintableTiles { get; set; } = true;

        public AcidSplashBehavior() { }

        public AcidSplashBehavior(int radius, float paintChance)
        {
            Radius = radius;
            PaintChance = paintChance;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            Vector2 center = projectile.Center;
            int tileX = (int)(center.X / 16f);
            int tileY = (int)(center.Y / 16f);

            for (int dx = -Radius; dx <= Radius; dx++)
            {
                for (int dy = -Radius; dy <= Radius; dy++)
                {
                    int x = tileX + dx;
                    int y = tileY + dy;

                    // 边界检查
                    if (x < 0 || x >= Main.maxTilesX || y < 0 || y >= Main.maxTilesY)
                        continue;

                    Tile tile = Main.tile[x, y];

                    // 跳过空物块
                    if (tile == null || !tile.HasTile)
                        continue;

                    // 跳过不可喷涂的物块
                    if (OnlyPaintableTiles && !IsPaintable(tile))
                        continue;

                    // 概率喷涂
                    if (Main.rand.NextFloat() >= PaintChance)
                        continue;

                    // 计算距离中心的距离，越近概率越高
                    float dist = new Vector2(dx, dy).Length();
                    if (dist > Radius) continue;
                    if (Main.rand.NextFloat() > 1f - (dist / (Radius + 1f)) * 0.5f)
                        continue;

                    // 喷涂物块颜色
                    WorldGen.paintTile(x, y, PaintColor);

                    // 添加液体
                    if (AddLiquid && LiquidAmount > 0)
                    {
                        // 只在非实心物块位置添加液体
                        bool isSolid = Main.tileSolid[tile.TileType];
                        if (!isSolid)
                        {
                            tile.LiquidType = LiquidType;
                            if (tile.LiquidAmount < LiquidAmount)
                                tile.LiquidAmount = LiquidAmount;
                        }
                    }

                    // 生成 Dust 粒子
                    if (SpawnDust)
                    {
                        for (int i = 0; i < DustCount / ((Radius * 2 + 1) * (Radius * 2 + 1) / 4 + 1); i++)
                        {
                            Vector2 dustPos = new Vector2(x * 16 + Main.rand.Next(16), y * 16 + Main.rand.Next(16));
                            int dustType = DustType >= 0 ? DustType : DustID.PoisonStaff;
                            Dust d = Dust.NewDustPerfect(dustPos, dustType,
                                Main.rand.NextVector2Circular(2f, 1f) - Vector2.UnitY * Main.rand.NextFloat(1f, 3f),
                                100, new Color(80, 255, 50), Main.rand.NextFloat(1f, 1.5f));
                            d.noGravity = true;
                        }
                    }
                }
            }

            // 强制刷新物块渲染
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                int range = Radius + 1;
                for (int dx = -range; dx <= range; dx++)
                {
                    for (int dy = -range; dy <= range; dy++)
                    {
                        int x = tileX + dx;
                        int y = tileY + dy;
                        if (x >= 0 && x < Main.maxTilesX && y >= 0 && y < Main.maxTilesY)
                        {
                            // 标记该区域需要重绘
                            if (Main.tile[x, y].HasTile)
                            {
                                WorldGen.SquareTileFrame(x, y);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 判断物块是否可喷涂（排除家具、旗帜等）
        /// </summary>
        private bool IsPaintable(Tile tile)
        {
            if (!tile.HasTile) return false;

            int type = tile.TileType;

            // 排除特定类型的物块
            // 家具类（frameImportant 的物块通常是家具/装饰）
            if (Main.tileFrameImportant[type]) return false;
            // 旗帜
            if (type == TileID.Banners) return false;
            // 树木
            if (TileID.Sets.IsATreeTrunk[type]) return false;
            // 植物/藤蔓
            if (TileID.Sets.IsVine[type]) return false;

            return true;
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
