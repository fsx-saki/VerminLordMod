using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 速度阻尼行为 — 每帧按比例衰减弹幕速度。
    ///
    /// 适用于：
    /// - 子弹飞行逐渐变慢（如弓箭、投掷物）
    /// - 爆炸碎片飞散后减速
    /// - 水/泥浆等粘稠介质的阻力模拟
    /// - 配合 AimBehavior（speed=0）使用，实现"初速度逐渐衰减"的效果
    ///
    /// 使用方式：
    /// <code>
    /// // 简单用法：每帧速度保留 97%
    /// Behaviors.Add(new DampingBehavior(0.97f));
    ///
    /// // 完整参数：阻尼 + 最低速度阈值 + 低于阈值时销毁
    /// Behaviors.Add(new DampingBehavior(0.96f)
    /// {
    ///     MinSpeed = 0.5f,
    ///     KillOnMinSpeed = true,
    ///     TimeLeftOnStop = 10,
    /// });
    ///
    /// // 各向异性阻尼（水平/垂直不同）
    /// Behaviors.Add(new DampingBehavior
    /// {
    ///     DampingX = 0.98f,  // 水平保留 98%
    ///     DampingY = 0.92f,  // 垂直保留 92%（重力感更强）
    /// });
    /// </code>
    ///
    /// 与 GravityBehavior 搭配效果最佳：重力加速 + 阻尼减速 → 终端速度自然形成。
    /// </summary>
    public class DampingBehavior : IBulletBehavior
    {
        public string Name => "Damping";

        // ===== 阻尼参数 =====

        /// <summary>
        /// 速度阻尼系数（0~1），每帧速度乘以该值。
        /// 1 = 无阻尼（不减速）
        /// 0.97 = 每帧保留 97%（约 30 帧后剩 40%）
        /// 0.95 = 每帧保留 95%（约 30 帧后剩 21%）
        /// 0.9 = 每帧保留 90%（约 30 帧后剩 4%）
        /// </summary>
        public float Damping { get; set; } = 0.97f;

        /// <summary>
        /// X 轴阻尼系数（覆盖 Damping，实现各向异性阻尼）。
        /// -1 = 使用 Damping 的全局值
        /// </summary>
        public float DampingX { get; set; } = -1f;

        /// <summary>
        /// Y 轴阻尼系数（覆盖 Damping，实现各向异性阻尼）。
        /// -1 = 使用 Damping 的全局值
        /// </summary>
        public float DampingY { get; set; } = -1f;

        // ===== 停止阈值参数 =====

        /// <summary>
        /// 最低速度阈值（像素/帧）。
        /// 当速度低于此值时触发停止逻辑。
        /// 0 = 禁用速度阈值检测。
        /// </summary>
        public float MinSpeed { get; set; } = 0f;

        /// <summary>
        /// 速度低于 MinSpeed 时是否销毁弹幕。
        /// 适用于"箭矢插在地上"等效果。
        /// </summary>
        public bool KillOnMinSpeed { get; set; } = false;

        /// <summary>
        /// 速度低于 MinSpeed 时是否将速度置零（停住）。
        /// 适用于"水球落地停住"等效果。
        /// </summary>
        public bool StopOnMinSpeed { get; set; } = false;

        /// <summary>
        /// 停住后剩余的存活时间（帧）。
        /// 仅 StopOnMinSpeed=true 时生效。
        /// </summary>
        public int TimeLeftOnStop { get; set; } = 30;

        /// <summary>
        /// 停住后是否禁用物块碰撞。
        /// 适合"插在地上的箭"不再被物块弹飞。
        /// </summary>
        public bool DisableTileCollideOnStop { get; set; } = false;

        // ===== 内部状态 =====

        /// <summary>是否已触发停止</summary>
        private bool _hasStopped;

        public DampingBehavior() { }

        /// <summary>
        /// 快速构造。
        /// </summary>
        /// <param name="damping">阻尼系数（0~1），越小阻尼越强</param>
        public DampingBehavior(float damping)
        {
            Damping = damping;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _hasStopped = false;
        }

        public void Update(Projectile projectile)
        {
            if (_hasStopped) return;

            // ── 应用阻尼 ──
            float dx = DampingX >= 0f ? DampingX : Damping;
            float dy = DampingY >= 0f ? DampingY : Damping;

            projectile.velocity.X *= dx;
            projectile.velocity.Y *= dy;

            // ── 速度阈值检测 ──
            if (MinSpeed > 0f)
            {
                float speed = projectile.velocity.Length();

                // 检查是否同时满足 X 和 Y 分量都低于阈值（避免斜向高速但分量小的情况）
                bool belowThreshold = speed < MinSpeed;

                if (belowThreshold && !_hasStopped)
                {
                    _hasStopped = true;

                    if (StopOnMinSpeed)
                    {
                        projectile.velocity = Vector2.Zero;
                        projectile.timeLeft = TimeLeftOnStop;

                        if (DisableTileCollideOnStop)
                        {
                            projectile.tileCollide = false;
                        }
                    }

                    if (KillOnMinSpeed)
                    {
                        projectile.Kill();
                    }
                }
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}
