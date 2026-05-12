using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 冰系拖尾行为 — 将 TrailManager + IceTrail 封装为 IBulletBehavior。
    ///
    /// 自动管理 IceTrail 的创建、更新、绘制和清理生命周期。
    /// 拖尾由虚影拖尾 + 两种粒子构成：
    /// - GhostTrail: 冰蓝色短虚影，使用 IceTrailGhost 贴图
    /// - 十字星星：静止滞留在弹幕路径上，闪烁淡出
    /// - 雪片：向后飞溅并受重力一簌簌大量落下，带水平飘摆
    ///
    /// 使用 Additive 混合呈现冰晶发光感。
    ///
    /// 使用示例：
    /// <code>
    /// Behaviors.Add(new IceTrailBehavior
    /// {
    ///     MaxStars = 40,
    ///     StarLife = 30,
    ///     MaxSnowflakes = 120,
    ///     SnowflakeClusterSize = 5,
    ///     AutoDraw = true,
    /// });
    /// </code>
    /// </summary>
    public class IceTrailBehavior : IBulletBehavior
    {
        public string Name => "IceTrail";

        public TrailManager TrailManager { get; } = new TrailManager();

        public IceTrail Trail { get; private set; }

        // ===== GhostTrail 配置 =====

        public bool EnableGhostTrail { get; set; } = true;

        public int GhostMaxPositions { get; set; } = 8;

        public int GhostRecordInterval { get; set; } = 2;

        public float GhostWidthScale { get; set; } = 0.3f;

        public float GhostLengthScale { get; set; } = 1.5f;

        public float GhostAlpha { get; set; } = 0.6f;

        public Color GhostColor { get; set; } = new Color(120, 200, 255, 180);

        // ===== 十字星星配置 =====

        public int MaxStars { get; set; } = 40;

        public int StarLife { get; set; } = 30;

        public float StarSize { get; set; } = 0.4f;

        public int StarSpawnInterval { get; set; } = 3;

        public Color StarColor { get; set; } = new Color(160, 220, 255, 220);

        public float StarRotSpeed { get; set; } = 0.01f;

        public float StarSpreadWidth { get; set; } = 8f;

        // ===== 雪片配置 =====

        public int MaxSnowflakes { get; set; } = 120;

        public int SnowflakeLife { get; set; } = 25;

        public float SnowflakeSize { get; set; } = 0.2f;

        public float SnowflakeGravity { get; set; } = 0.1f;

        public float SnowflakeAirResistance { get; set; } = 0.98f;

        public Color SnowflakeColor { get; set; } = new Color(200, 240, 255, 180);

        public int SnowflakeClusterSize { get; set; } = 5;

        public float SnowflakeSplashSpeed { get; set; } = 3f;

        public float SnowflakeSplashAngle { get; set; } = 1.2f;

        public float SnowflakeSpawnChance { get; set; } = 0.7f;

        public float InertiaFactor { get; set; } = 0.3f;

        public float RandomSpread { get; set; } = 6f;

        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        // ===== 绘制控制 =====

        public bool AutoDraw { get; set; } = true;

        public bool SuppressDefaultDraw { get; set; } = false;

        public IceTrailBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Trail = new IceTrail
            {
                EnableGhostTrail = EnableGhostTrail,
                GhostMaxPositions = GhostMaxPositions,
                GhostRecordInterval = GhostRecordInterval,
                GhostWidthScale = GhostWidthScale,
                GhostLengthScale = GhostLengthScale,
                GhostAlpha = GhostAlpha,
                GhostColor = GhostColor,

                MaxStars = MaxStars,
                StarLife = StarLife,
                StarSize = StarSize,
                StarSpawnInterval = StarSpawnInterval,
                StarColor = StarColor,
                StarRotSpeed = StarRotSpeed,
                StarSpreadWidth = StarSpreadWidth,

                MaxSnowflakes = MaxSnowflakes,
                SnowflakeLife = SnowflakeLife,
                SnowflakeSize = SnowflakeSize,
                SnowflakeGravity = SnowflakeGravity,
                SnowflakeAirResistance = SnowflakeAirResistance,
                SnowflakeColor = SnowflakeColor,
                SnowflakeClusterSize = SnowflakeClusterSize,
                SnowflakeSplashSpeed = SnowflakeSplashSpeed,
                SnowflakeSplashAngle = SnowflakeSplashAngle,
                SnowflakeSpawnChance = SnowflakeSpawnChance,
                InertiaFactor = InertiaFactor,
                RandomSpread = RandomSpread,
                SpawnOffset = SpawnOffset,
            };

            TrailManager.Add(Trail);
        }

        public void Update(Projectile projectile)
        {
            TrailManager.Update(projectile.Center, projectile.velocity);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            TrailManager.Clear();
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            if (AutoDraw)
            {
                TrailManager.Draw(spriteBatch);
            }
            return !SuppressDefaultDraw;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}