using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 液体反应行为 — 弹幕碰到不同液体时触发不同效果。
    ///
    /// 液体检测逻辑（每帧在 Update 中检查）：
    /// - 水/蜂蜜：触发一次后销毁弹幕（融入液体）
    /// - 岩浆：产生蒸汽粒子后销毁（汽化）
    /// - 微光（Shimmer）：反弹（反转速度方向）
    ///
    /// 检测方式：直接检查 projectile 所在物块的 tile.liquid 属性，
    /// 不依赖 projectile.wet（因为 ignoreWater=true 时 wet 不会被设置）。
    ///
    /// 注意：检测范围覆盖 projectile 的整个 hitbox（左上、右上、左下、右下、中心），
    /// 避免弹幕刚好在液体方块边缘时检测不到。
    /// </summary>
    public class LiquidReactionBehavior : IBulletBehavior
    {
        public string Name => "LiquidReaction";

        // ===== 可配置参数 =====

        /// <summary>是否启用水/蜂蜜融合效果</summary>
        public bool EnableMerge { get; set; } = true;

        /// <summary>是否启用岩浆汽化效果</summary>
        public bool EnableVaporize { get; set; } = true;

        /// <summary>是否启用微光反弹效果</summary>
        public bool EnableShimmerBounce { get; set; } = true;

        /// <summary>蒸汽 Dust 类型</summary>
        public int SteamDustType { get; set; } = DustID.Cloud;

        /// <summary>蒸汽粒子数量</summary>
        public int SteamDustCount { get; set; } = 15;

        /// <summary>蒸汽粒子速度</summary>
        public float SteamDustSpeed { get; set; } = 4f;

        /// <summary>微光反弹后速度倍率</summary>
        public float ShimmerBounceFactor { get; set; } = 0.8f;

        /// <summary>微光反弹 Dust 类型</summary>
        public int ShimmerDustType { get; set; } = DustID.ShimmerSpark;

        /// <summary>微光反弹粒子数量</summary>
        public int ShimmerDustCount { get; set; } = 10;

        /// <summary>融合时生成的 Dust 类型（水/蜂蜜）</summary>
        public int MergeDustType { get; set; } = DustID.Water;

        /// <summary>融合时粒子数量</summary>
        public int MergeDustCount { get; set; } = 8;

        /// <summary>检测间隔（帧），避免每帧都检查 tile</summary>
        public int CheckInterval { get; set; } = 3;

        /// <summary>
        /// 检测范围半径（tile 格数）。
        /// 0 = 只检测弹幕中心所在 tile
        /// 1 = 检测中心 + 上下左右 4 个相邻 tile
        /// 2 = 检测 5x5 范围
        /// </summary>
        public int ScanRadius { get; set; } = 1;

        // 内部状态
        private bool _hasReacted = false;
        private bool _hasShimmerBounced = false;
        private int _checkCounter = 0;

        public LiquidReactionBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _hasReacted = false;
            _hasShimmerBounced = false;
            _checkCounter = 0;
        }

        /// <summary>
        /// 检测指定 tile 坐标是否有液体。
        /// 返回液体类型（0=水, 1=岩浆, 2=蜂蜜, 3=微光），无液体返回 -1。
        /// </summary>
        private int GetLiquidTypeAt(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
                return -1;

            Tile tile = Main.tile[tileX, tileY];
            if (tile == null || tile.LiquidAmount <= 0)
                return -1;

            return tile.LiquidType;
        }

        /// <summary>
        /// 在弹幕周围扫描液体。
        /// 返回找到的液体类型，优先返回优先级高的液体（岩浆 > 微光 > 蜂蜜 > 水）。
        /// </summary>
        private int ScanForLiquid(Projectile projectile)
        {
            int centerX = (int)(projectile.Center.X / 16f);
            int centerY = (int)(projectile.Center.Y / 16f);

            // 优先级：岩浆(1) > 微光(3) > 蜂蜜(2) > 水(0)
            // 按优先级从高到低检查
            int[] priorityOrder = new int[] { 1, 3, 2, 0 };

            for (int dx = -ScanRadius; dx <= ScanRadius; dx++)
            {
                for (int dy = -ScanRadius; dy <= ScanRadius; dy++)
                {
                    int tileX = centerX + dx;
                    int tileY = centerY + dy;
                    int liquidType = GetLiquidTypeAt(tileX, tileY);
                    if (liquidType >= 0)
                        return liquidType;
                }
            }

            return -1;
        }

        public void Update(Projectile projectile)
        {
            // 已反应则跳过
            if (_hasReacted) return;

            // 间隔检测，优化性能
            _checkCounter++;
            if (_checkCounter % CheckInterval != 0) return;

            // 扫描弹幕周围的液体
            int liquidType = ScanForLiquid(projectile);
            if (liquidType < 0)
            {
                // 不在液体中，重置微光反弹标记以便下次进入微光时能再次反弹
                _hasShimmerBounced = false;
                return;
            }

            bool inWater = liquidType == 0;
            bool inLava = liquidType == 1;
            bool inHoney = liquidType == 2;
            bool inShimmer = liquidType == 3;

            // ===== 微光：反弹 =====
            if (inShimmer && EnableShimmerBounce && !_hasShimmerBounced)
            {
                _hasShimmerBounced = true;

                // 反转速度方向并减速
                projectile.velocity *= -ShimmerBounceFactor;

                // 生成微光粒子
                for (int i = 0; i < ShimmerDustCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                    Dust d = Dust.NewDustPerfect(
                        projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        ShimmerDustType,
                        vel, 100, default, Main.rand.NextFloat(1f, 1.8f)
                    );
                    d.noGravity = true;
                }

                // 播放音效
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash, projectile.Center);

                return; // 不销毁，继续飞行
            }

            // ===== 岩浆：汽化 =====
            if (inLava && EnableVaporize)
            {
                _hasReacted = true;

                // 生成蒸汽粒子（大量白色/灰色 Dust 向上飘）
                for (int i = 0; i < SteamDustCount; i++)
                {
                    Vector2 vel = new Vector2(
                        Main.rand.NextFloat(-SteamDustSpeed, SteamDustSpeed),
                        Main.rand.NextFloat(-SteamDustSpeed * 1.5f, -SteamDustSpeed * 0.3f)
                    );
                    Dust d = Dust.NewDustPerfect(
                        projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
                        SteamDustType,
                        vel, 150, Color.LightGray, Main.rand.NextFloat(1.2f, 2.0f)
                    );
                    d.noGravity = true;
                    d.fadeIn = 0.5f;
                }

                // 额外红色火星
                for (int i = 0; i < 5; i++)
                {
                    Dust d = Dust.NewDustPerfect(
                        projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                        DustID.Torch,
                        Main.rand.NextVector2Circular(2f, 2f),
                        200, default, Main.rand.NextFloat(0.8f, 1.5f)
                    );
                    d.noGravity = true;
                }

                // 播放音效
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash, projectile.Center);

                // 销毁弹幕
                projectile.Kill();
                return;
            }

            // ===== 水/蜂蜜：融入 =====
            if ((inWater || inHoney) && EnableMerge)
            {
                _hasReacted = true;

                // 生成水花/蜂蜜粒子
                int dustType = inHoney ? DustID.Honey : MergeDustType;
                Color dustColor = inHoney ? new Color(255, 200, 50) : new Color(80, 180, 255);

                for (int i = 0; i < MergeDustCount; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(3f, 3f) - Vector2.UnitY * Main.rand.NextFloat(1f, 3f);
                    Dust d = Dust.NewDustPerfect(
                        projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                        dustType,
                        vel, 100, dustColor, Main.rand.NextFloat(0.8f, 1.5f)
                    );
                    d.noGravity = true;
                }

                // 播放水花音效
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Splash, projectile.Center);

                // 销毁弹幕
                projectile.Kill();
                return;
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            return true;
        }
    }
}
