using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 区域生成行为 — 弹幕在指定形状区域内随机位置生成，并设置初始速度指向目标。
    ///
    /// 支持多种形状：
    /// - Ring（圆环）：在指定半径范围的圆环上随机位置生成
    /// - Circle（圆形）：在指定半径的圆形区域内随机位置生成
    /// - Rectangle（矩形）：在指定宽高的矩形区域内随机位置生成
    /// - Sector（扇形）：在指定角度范围的扇形区域内随机位置生成
    ///
    /// 适用于"从四周汇聚到中心"类攻击方式，例如：
    /// - 法阵从圆环上汇聚水弹
    /// - 从矩形区域随机位置发射弹幕
    /// - 扇形散射
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new RegionSpawnBehavior
    /// {
    ///     Shape = SpawnShape.Ring,
    ///     InnerRadius = 50f,       // 圆环内半径
    ///     OuterRadius = 90f,       // 圆环外半径
    ///     TargetCenter = () => Main.MouseWorld,  // 目标位置（动态）
    ///     InitialSpeed = 8f,
    /// });
    /// </code>
    /// </summary>
    public class RegionSpawnBehavior : IBulletBehavior
    {
        public string Name => "RegionSpawn";

        // ===== 形状类型 =====

        public enum SpawnShape
        {
            /// <summary>圆环：在指定半径范围的圆环上随机位置生成</summary>
            Ring,
            /// <summary>圆形：在指定半径的圆形区域内随机位置生成</summary>
            Circle,
            /// <summary>矩形：在指定宽高的矩形区域内随机位置生成</summary>
            Rectangle,
            /// <summary>扇形：在指定角度范围的扇形区域内随机位置生成</summary>
            Sector,
        }

        // ===== 形状参数 =====

        /// <summary>生成形状类型</summary>
        public SpawnShape Shape { get; set; } = SpawnShape.Ring;

        /// <summary>圆环/圆形内半径（像素），仅 Ring/Sector 有效</summary>
        public float InnerRadius { get; set; } = 50f;

        /// <summary>圆环/圆形外半径（像素）</summary>
        public float OuterRadius { get; set; } = 90f;

        /// <summary>矩形宽度（像素），仅 Rectangle 有效</summary>
        public float RectWidth { get; set; } = 100f;

        /// <summary>矩形高度（像素），仅 Rectangle 有效</summary>
        public float RectHeight { get; set; } = 100f;

        /// <summary>扇形起始角度（弧度），仅 Sector 有效</summary>
        public float SectorStartAngle { get; set; } = -MathHelper.PiOver4;

        /// <summary>扇形角度范围（弧度），仅 Sector 有效</summary>
        public float SectorAngle { get; set; } = MathHelper.PiOver2;

        // ===== 目标与速度 =====

        /// <summary>
        /// 目标位置提供函数（返回目标坐标）。
        /// 弹幕生成时初始速度指向此目标。
        /// 如果为 null，则不修改速度方向。
        /// </summary>
        public System.Func<Vector2> TargetCenter { get; set; } = null;

        /// <summary>初始速度大小（像素/帧）</summary>
        public float InitialSpeed { get; set; } = 8f;

        /// <summary>
        /// 生成中心位置提供函数。
        /// 如果为 null，则使用弹幕生成时的位置作为中心。
        /// </summary>
        public System.Func<Vector2> SpawnCenter { get; set; } = null;

        /// <summary>是否在生成时应用随机偏移（如果为 false，可在后续手动触发）</summary>
        public bool ApplyOnSpawn { get; set; } = true;

        /// <summary>是否自动旋转到速度方向</summary>
        public bool AutoRotate { get; set; } = true;

        /// <summary>旋转偏移量（弧度）</summary>
        public float RotationOffset { get; set; } = 0f;

        public RegionSpawnBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            if (!ApplyOnSpawn) return;

            // 1. 确定生成中心
            Vector2 center = SpawnCenter?.Invoke() ?? projectile.Center;

            // 2. 在指定形状区域内随机生成位置
            Vector2 spawnPos = GetRandomPosition(center);
            projectile.Center = spawnPos;
            projectile.position = spawnPos - new Vector2(projectile.width * 0.5f, projectile.height * 0.5f);

            // 3. 设置初始速度指向目标
            if (TargetCenter != null && InitialSpeed > 0f)
            {
                Vector2 target = TargetCenter.Invoke();
                Vector2 toTarget = target - projectile.Center;
                if (toTarget != Vector2.Zero)
                {
                    projectile.velocity = toTarget.SafeNormalize(Vector2.Zero) * InitialSpeed;
                }
            }

            // 4. 自动旋转
            if (AutoRotate && projectile.velocity != Vector2.Zero)
            {
                projectile.rotation = projectile.velocity.ToRotation() + RotationOffset;
            }
        }

        /// <summary>
        /// 根据形状类型获取随机位置
        /// </summary>
        private Vector2 GetRandomPosition(Vector2 center)
        {
            switch (Shape)
            {
                case SpawnShape.Ring:
                    return GetRandomRingPosition(center);

                case SpawnShape.Circle:
                    return GetRandomCirclePosition(center);

                case SpawnShape.Rectangle:
                    return GetRandomRectanglePosition(center);

                case SpawnShape.Sector:
                    return GetRandomSectorPosition(center);

                default:
                    return GetRandomRingPosition(center);
            }
        }

        /// <summary>
        /// 在圆环上随机位置生成（内半径 ~ 外半径）
        /// </summary>
        private Vector2 GetRandomRingPosition(Vector2 center)
        {
            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float radius = Main.rand.NextFloat(InnerRadius, OuterRadius);
            return center + angle.ToRotationVector2() * radius;
        }

        /// <summary>
        /// 在圆形区域内随机位置生成
        /// </summary>
        private Vector2 GetRandomCirclePosition(Vector2 center)
        {
            return center + Main.rand.NextVector2Circular(OuterRadius, OuterRadius);
        }

        /// <summary>
        /// 在矩形区域内随机位置生成
        /// </summary>
        private Vector2 GetRandomRectanglePosition(Vector2 center)
        {
            float x = Main.rand.NextFloat(-RectWidth * 0.5f, RectWidth * 0.5f);
            float y = Main.rand.NextFloat(-RectHeight * 0.5f, RectHeight * 0.5f);
            return center + new Vector2(x, y);
        }

        /// <summary>
        /// 在扇形区域内随机位置生成
        /// </summary>
        private Vector2 GetRandomSectorPosition(Vector2 center)
        {
            float angle = SectorStartAngle + Main.rand.NextFloat(0f, SectorAngle);
            float radius = Main.rand.NextFloat(InnerRadius, OuterRadius);
            return center + angle.ToRotationVector2() * radius;
        }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
