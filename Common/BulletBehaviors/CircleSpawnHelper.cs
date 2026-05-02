using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 法阵生成辅助类 — 提供多种从法阵生成弹幕的方式。
    ///
    /// 支持的模式：
    /// - SpawnFromEdge：从法阵边缘随机位置生成弹幕，飞向中心（汇聚）
    /// - SpawnRing：从法阵边缘均匀分布生成弹幕，飞向中心（环状汇聚）
    /// - SpawnHomingFromEdge：从法阵边缘生成追踪弹幕，自动追踪敌人
    /// - SpawnTangential：从法阵边缘沿切线方向甩出弹幕（旋转甩出）
    /// - SpawnBurst：法阵爆发式向四面八方发射弹幕
    /// - SpawnSpiral：螺旋式向外发射弹幕
    /// </summary>
    public static class CircleSpawnHelper
    {
        /// <summary>
        /// 获取或创建 IEntitySource
        /// </summary>
        private static IEntitySource GetSource(int owner, IEntitySource source)
        {
            if (source != null) return source;
            if (owner >= 0 && owner < Main.maxPlayers)
            {
                var player = Main.player[owner];
                if (player != null && player.active)
                    return player.GetSource_FromThis();
            }
            return null;
        }

        // ================================================================
        //  模式 1：汇聚 — 从法阵边缘随机位置生成弹幕，飞向中心
        // ================================================================

        /// <summary>
        /// 从法阵边缘随机位置生成弹幕，飞向法阵中心。
        /// 适用于"冰碎片汇聚"、"灵魂汇聚"等效果。
        /// </summary>
        public static int SpawnFromEdge(
            Vector2 center,
            float innerRadius,
            float outerRadius,
            int projType,
            float speed = 6f,
            int owner = 255,
            IEntitySource source = null,
            int damage = 0,
            float knockback = 0f,
            System.Action<int> onSpawn = null)
        {
            source = GetSource(owner, source);
            if (source == null) return -1;

            float angle = Main.rand.NextFloat(MathHelper.TwoPi);
            float distance = Main.rand.NextFloat(innerRadius, outerRadius);
            Vector2 spawnPos = center + angle.ToRotationVector2() * distance;
            Vector2 vel = (center - spawnPos).SafeNormalize(Vector2.Zero) * speed;

            int projIndex = Projectile.NewProjectile(
                source, spawnPos, vel,
                projType, damage, knockback, owner);

            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                onSpawn?.Invoke(projIndex);

            return projIndex;
        }

        // ================================================================
        //  模式 2：环状汇聚 — 从法阵边缘均匀分布生成弹幕，飞向中心
        // ================================================================

        /// <summary>
        /// 从法阵边缘均匀分布生成多个弹幕，全部飞向法阵中心。
        /// 适用于"万箭归宗"、"环状冰刺"等效果。
        /// </summary>
        public static int[] SpawnRing(
            Vector2 center,
            float radius,
            int projType,
            int count,
            float speed = 6f,
            int owner = 255,
            IEntitySource source = null,
            int damage = 0,
            float knockback = 0f,
            float angleOffset = 0f,
            System.Action<int> onSpawn = null)
        {
            source = GetSource(owner, source);
            if (source == null) return System.Array.Empty<int>();

            int[] result = new int[count];

            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + angleOffset;
                Vector2 spawnPos = center + angle.ToRotationVector2() * radius;
                Vector2 vel = (center - spawnPos).SafeNormalize(Vector2.Zero) * speed;

                int projIndex = Projectile.NewProjectile(
                    source, spawnPos, vel,
                    projType, damage, knockback, owner);

                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                    onSpawn?.Invoke(projIndex);

                result[i] = projIndex;
            }

            return result;
        }

        // ================================================================
        //  模式 3：追踪 — 从法阵边缘生成追踪弹幕，自动追踪敌人
        // ================================================================

        /// <summary>
        /// 从法阵边缘随机位置生成追踪弹幕，弹幕自带追踪 AI 飞向最近敌人。
        /// 适用于"法阵持续产生追踪弹幕"的效果。
        /// 弹幕本身需要实现追踪逻辑（如 HomingBehavior）。
        /// </summary>
        /// <param name="center">法阵中心</param>
        /// <param name="innerRadius">最小生成半径</param>
        /// <param name="outerRadius">最大生成半径</param>
        /// <param name="projType">弹幕类型（应自带追踪 AI）</param>
        /// <param name="speed">初始速度</param>
        /// <param name="owner">弹幕所有者</param>
        /// <param name="source">生成源</param>
        /// <param name="damage">伤害</param>
        /// <param name="knockback">击退</param>
        /// <param name="initialDirection">初始飞行方向（默认随机）</param>
        /// <param name="onSpawn">生成后的回调</param>
        /// <returns>生成的弹幕索引</returns>
        public static int SpawnHomingFromEdge(
            Vector2 center,
            float innerRadius,
            float outerRadius,
            int projType,
            float speed = 6f,
            int owner = 255,
            IEntitySource source = null,
            int damage = 0,
            float knockback = 0f,
            float? initialDirection = null,
            System.Action<int> onSpawn = null)
        {
            source = GetSource(owner, source);
            if (source == null) return -1;

            float angle = initialDirection ?? Main.rand.NextFloat(MathHelper.TwoPi);
            float distance = Main.rand.NextFloat(innerRadius, outerRadius);
            Vector2 spawnPos = center + angle.ToRotationVector2() * distance;

            // 追踪弹幕需要一个初始速度方向（即使之后会被追踪 AI 覆盖）
            Vector2 vel = angle.ToRotationVector2() * speed;

            int projIndex = Projectile.NewProjectile(
                source, spawnPos, vel,
                projType, damage, knockback, owner);

            if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                onSpawn?.Invoke(projIndex);

            return projIndex;
        }

        // ================================================================
        //  模式 4：切线甩出 — 从法阵边缘沿切线方向甩出弹幕
        // ================================================================

        /// <summary>
        /// 从法阵边缘沿切线方向甩出弹幕，模拟"旋转甩出"的效果。
        /// 适用于"法阵旋转甩出弹幕"、"回旋镖"等效果。
        /// </summary>
        /// <param name="center">法阵中心</param>
        /// <param name="radius">生成半径（法阵半径）</param>
        /// <param name="projType">弹幕类型</param>
        /// <param name="speed">切线速度</param>
        /// <param name="count">一次甩出的数量（均匀分布在圆周上）</param>
        /// <param name="rotationAngle">法阵当前旋转角度（用于计算切线方向）</param>
        /// <param name="tangentSign">切线方向符号（1=顺时针, -1=逆时针）</param>
        /// <param name="outwardBias">向外偏移量（0=纯切线, >0=向外扩散）</param>
        /// <param name="owner">弹幕所有者</param>
        /// <param name="source">生成源</param>
        /// <param name="damage">伤害</param>
        /// <param name="knockback">击退</param>
        /// <param name="onSpawn">生成后的回调</param>
        /// <returns>生成的弹幕索引数组</returns>
        public static int[] SpawnTangential(
            Vector2 center,
            float radius,
            int projType,
            float speed = 8f,
            int count = 1,
            float rotationAngle = 0f,
            float tangentSign = 1f,
            float outwardBias = 0f,
            int owner = 255,
            IEntitySource source = null,
            int damage = 0,
            float knockback = 0f,
            System.Action<int> onSpawn = null)
        {
            source = GetSource(owner, source);
            if (source == null) return System.Array.Empty<int>();

            int[] result = new int[count];

            for (int i = 0; i < count; i++)
            {
                // 弹幕在圆周上的位置角度
                float posAngle = MathHelper.TwoPi * i / count + rotationAngle;
                Vector2 spawnPos = center + posAngle.ToRotationVector2() * radius;

                // 切线方向 = 位置角度 + 90°（顺时针）或 -90°（逆时针）
                float tangentAngle = posAngle + tangentSign * MathHelper.PiOver2;

                // 如果有向外偏移，混合切线方向和径向向外方向
                Vector2 vel;
                if (outwardBias > 0f)
                {
                    Vector2 tangentDir = tangentAngle.ToRotationVector2();
                    Vector2 outwardDir = posAngle.ToRotationVector2();
                    vel = Vector2.Lerp(tangentDir, outwardDir, outwardBias).SafeNormalize(Vector2.Zero) * speed;
                }
                else
                {
                    vel = tangentAngle.ToRotationVector2() * speed;
                }

                int projIndex = Projectile.NewProjectile(
                    source, spawnPos, vel,
                    projType, damage, knockback, owner);

                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                    onSpawn?.Invoke(projIndex);

                result[i] = projIndex;
            }

            return result;
        }

        // ================================================================
        //  模式 5：爆发 — 法阵爆发式向四面八方发射弹幕
        // ================================================================

        /// <summary>
        /// 法阵爆发式向四面八方发射弹幕，类似"爆炸开花"效果。
        /// 适用于"法阵碎裂"、"爆发冲击波"等效果。
        /// </summary>
        /// <param name="center">法阵中心</param>
        /// <param name="projType">弹幕类型</param>
        /// <param name="count">弹幕数量</param>
        /// <param name="speed">速度</param>
        /// <param name="speedVariation">速度随机变化量 (±)</param>
        /// <param name="angleSpread">角度随机偏移</param>
        /// <param name="owner">弹幕所有者</param>
        /// <param name="source">生成源</param>
        /// <param name="damage">伤害</param>
        /// <param name="knockback">击退</param>
        /// <param name="onSpawn">生成后的回调</param>
        /// <returns>生成的弹幕索引数组</returns>
        public static int[] SpawnBurst(
            Vector2 center,
            int projType,
            int count = 12,
            float speed = 8f,
            float speedVariation = 2f,
            float angleSpread = 0f,
            int owner = 255,
            IEntitySource source = null,
            int damage = 0,
            float knockback = 0f,
            System.Action<int> onSpawn = null)
        {
            source = GetSource(owner, source);
            if (source == null) return System.Array.Empty<int>();

            int[] result = new int[count];

            for (int i = 0; i < count; i++)
            {
                float baseAngle = MathHelper.TwoPi * i / count;
                float angle = baseAngle + Main.rand.NextFloat(-angleSpread, angleSpread);
                float actualSpeed = speed + Main.rand.NextFloat(-speedVariation, speedVariation);
                Vector2 vel = angle.ToRotationVector2() * actualSpeed;

                int projIndex = Projectile.NewProjectile(
                    source, center, vel,
                    projType, damage, knockback, owner);

                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                    onSpawn?.Invoke(projIndex);

                result[i] = projIndex;
            }

            return result;
        }

        // ================================================================
        //  模式 6：螺旋 — 螺旋式向外发射弹幕
        // ================================================================

        /// <summary>
        /// 螺旋式向外发射弹幕，每帧沿螺旋路径生成。
        /// 适用于"螺旋弹幕"、"旋转光轮"等效果。
        /// </summary>
        /// <param name="center">法阵中心</param>
        /// <param name="projType">弹幕类型</param>
        /// <param name="count">一次生成的弹幕数量</param>
        /// <param name="startRadius">起始半径</param>
        /// <param name="endRadius">结束半径</param>
        /// <param name="speed">飞行速度（径向向外）</param>
        /// <param name="rotationSpeed">螺旋旋转速度</param>
        /// <param name="totalAngle">总旋转角度</param>
        /// <param name="owner">弹幕所有者</param>
        /// <param name="source">生成源</param>
        /// <param name="damage">伤害</param>
        /// <param name="knockback">击退</param>
        /// <param name="onSpawn">生成后的回调</param>
        /// <returns>生成的弹幕索引数组</returns>
        public static int[] SpawnSpiral(
            Vector2 center,
            int projType,
            int count = 8,
            float startRadius = 20f,
            float endRadius = 120f,
            float speed = 4f,
            float rotationSpeed = 0.1f,
            float totalAngle = MathHelper.TwoPi * 2,
            int owner = 255,
            IEntitySource source = null,
            int damage = 0,
            float knockback = 0f,
            System.Action<int> onSpawn = null)
        {
            source = GetSource(owner, source);
            if (source == null) return System.Array.Empty<int>();

            int[] result = new int[count];

            for (int i = 0; i < count; i++)
            {
                float t = count > 1 ? (float)i / (count - 1) : 0f;
                float radius = MathHelper.Lerp(startRadius, endRadius, t);
                float angle = t * totalAngle;

                Vector2 spawnPos = center + angle.ToRotationVector2() * radius;

                // 速度方向 = 径向向外 + 切向旋转
                Vector2 radialDir = angle.ToRotationVector2();
                Vector2 tangentDir = (angle + MathHelper.PiOver2).ToRotationVector2();
                Vector2 vel = (radialDir * speed + tangentDir * radius * rotationSpeed);

                int projIndex = Projectile.NewProjectile(
                    source, spawnPos, vel,
                    projType, damage, knockback, owner);

                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                    onSpawn?.Invoke(projIndex);

                result[i] = projIndex;
            }

            return result;
        }
    }
}
