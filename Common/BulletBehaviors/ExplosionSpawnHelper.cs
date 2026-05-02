using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 爆炸产弹幕辅助类 — 抽象出 BloodHandprintsProj.OnKill 中的弹幕生成逻辑。
    ///
    /// 用于"弹幕销毁时产生大量子弹幕向四面八方飞散"的效果，例如：
    /// - 血手印销毁时产生 BloodDropProj
    /// - 酸液弹幕销毁时产生酸液滴
    /// - 任何需要"爆炸产弹幕"的场景
    ///
    /// 使用方式：
    /// <code>
    /// public override void OnKill(int timeLeft)
    /// {
    ///     ExplosionSpawnHelper.SpawnProjectiles(
    ///         Projectile.Center,
    ///         ModContent.ProjectileType<MyChildProj>(),
    ///         count: 10,
    ///         speedMin: 4f, speedMax: 10f,
    ///         owner: Projectile.owner);
    /// }
    /// </code>
    /// </summary>
    public static class ExplosionSpawnHelper
    {
        /// <summary>
        /// 向四面八方生成子弹幕
        /// </summary>
        /// <param name="center">爆炸中心</param>
        /// <param name="projType">子弹幕类型</param>
        /// <param name="count">生成数量</param>
        /// <param name="speedMin">最小速度</param>
        /// <param name="speedMax">最大速度</param>
        /// <param name="owner">弹幕所有者</param>
        /// <param name="damage">伤害（默认 0）</param>
        /// <param name="spreadRadius">生成位置随机偏移半径（默认 10）</param>
        /// <param name="angleSpread">角度随机偏移（默认 ±0.3）</param>
        /// <param name="source">生成源（默认 null，使用 FromThis）</param>
        /// <returns>生成的弹幕索引数组</returns>
        public static int[] SpawnProjectiles(
            Vector2 center,
            int projType,
            int count,
            float speedMin = 4f,
            float speedMax = 10f,
            int owner = 255,
            int damage = 0,
            float spreadRadius = 10f,
            float angleSpread = 0.3f,
            IEntitySource source = null)
        {
            if (source == null && owner >= 0 && owner < Main.maxPlayers)
            {
                var player = Main.player[owner];
                if (player != null && player.active)
                {
                    source = player.GetSource_FromThis();
                }
            }

            if (source == null)
                return System.Array.Empty<int>();

            int[] result = new int[count];

            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPos = center + Main.rand.NextVector2Circular(spreadRadius, spreadRadius);
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-angleSpread, angleSpread);
                float speed = Main.rand.NextFloat(speedMin, speedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;

                result[i] = Projectile.NewProjectile(
                    source, spawnPos, vel,
                    projType, damage, 0f, owner);
            }

            return result;
        }

        /// <summary>
        /// 生成血液飞溅粒子（Dust 爆炸效果，类似 LiquidBurstBehavior）
        /// </summary>
        /// <param name="center">爆炸中心</param>
        /// <param name="dustType">Dust 类型（默认 Blood）</param>
        /// <param name="count">粒子数量</param>
        /// <param name="speedMin">最小速度</param>
        /// <param name="speedMax">最大速度</param>
        /// <param name="colorStart">起始颜色</param>
        /// <param name="colorEnd">结束颜色</param>
        /// <param name="scaleMin">最小大小</param>
        /// <param name="scaleMax">最大大小</param>
        /// <param name="noGravity">是否无重力</param>
        public static void SpawnBurstDust(
            Vector2 center,
            int dustType = DustID.Blood,
            int count = 20,
            float speedMin = 3f,
            float speedMax = 8f,
            Color? colorStart = null,
            Color? colorEnd = null,
            float scaleMin = 0.6f,
            float scaleMax = 1.5f,
            bool noGravity = true)
        {
            Color cStart = colorStart ?? new Color(180, 20, 20, 200);
            Color cEnd = colorEnd ?? new Color(80, 0, 0, 0);

            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(speedMin, speedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;
                float scale = Main.rand.NextFloat(scaleMin, scaleMax);
                Color dustColor = Color.Lerp(cStart, cEnd, Main.rand.NextFloat(0f, 0.6f));

                int dustId = Dust.NewDust(
                    center - new Vector2(8f), 16, 16,
                    dustType, vel.X, vel.Y, 100, dustColor, scale
                );
                if (dustId >= 0 && dustId < Main.dust.Length)
                {
                    Dust d = Main.dust[dustId];
                    d.noGravity = noGravity;
                    d.velocity = vel;
                    d.position = center - new Vector2(4f);
                    d.fadeIn = 0.5f;
                }
            }
        }

        /// <summary>
        /// 生成血雾粒子（小、慢、半透明）
        /// </summary>
        /// <param name="center">爆炸中心</param>
        /// <param name="dustType">Dust 类型（默认 Blood）</param>
        /// <param name="count">粒子数量</param>
        /// <param name="speedMin">最小速度</param>
        /// <param name="speedMax">最大速度</param>
        /// <param name="color">颜色</param>
        /// <param name="scaleMin">最小大小</param>
        /// <param name="scaleMax">最大大小</param>
        public static void SpawnMistDust(
            Vector2 center,
            int dustType = DustID.Blood,
            int count = 10,
            float speedMin = 1f,
            float speedMax = 4f,
            Color? color = null,
            float scaleMin = 0.3f,
            float scaleMax = 0.6f)
        {
            Color mistColor = color ?? new Color(100, 0, 0, 80);

            for (int i = 0; i < count; i++)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(speedMin, speedMax);
                Vector2 vel = angle.ToRotationVector2() * speed;

                int dustId = Dust.NewDust(
                    center - new Vector2(8f), 16, 16,
                    dustType, vel.X, vel.Y, 80, mistColor,
                    Main.rand.NextFloat(scaleMin, scaleMax)
                );
                if (dustId >= 0 && dustId < Main.dust.Length)
                {
                    Dust d = Main.dust[dustId];
                    d.noGravity = true;
                    d.velocity = vel;
                    d.position = center - new Vector2(4f);
                    d.fadeIn = 0.3f;
                }
            }
        }
    }
}
