using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using VerminLordMod.Common.SummonBehaviors.Selectors;
using VerminLordMod.Common.SummonBehaviors.Styles;

namespace VerminLordMod.Common.SummonBehaviors.Modules
{
    /// <summary>
    /// 追击模块 — 组合运动方式和目标选择器，形成完整的"追击敌人"行为。
    ///
    /// 这是分层架构的第三层：MovementStyle + TargetSelector = MovementModule。
    ///
    /// 行为逻辑：
    /// - 有目标时：使用 MovementStyle 飞向 TargetSelector 选中的目标
    /// - 无目标时：切换到 IdleSelector（跟随玩家）
    /// - 目标超出 MaxChaseRange 时：放弃追击，返回玩家身边
    ///
    /// 组合示例：
    ///   ChaseModule(FlyStyle, NearestEnemySelector)  → 飞行追击最近敌人
    ///   ChaseModule(WalkStyle, NearestEnemySelector) → 步行追击最近敌人
    ///   ChaseModule(FlyStyle, LockedEnemySelector)   → 飞行追击锁定敌人
    ///
    /// 可配置参数：
    /// - MovementStyle: 运动方式（怎么动）
    /// - TargetSelector: 目标选择器（去哪）
    /// - ChaseSpeed: 追击速度
    /// - MaxChaseRange: 最大追击范围（超出则放弃）
    /// - IdleFollowDistance: 无目标时跟随玩家的距离
    /// - IdleFollowSpeed: 无目标时跟随玩家的速度
    /// </summary>
    public class ChaseModule : ISummonBehavior
    {
        public string Name => "ChaseModule";

        public IMovementStyle MovementStyle { get; set; }
        public ITargetSelector TargetSelector { get; set; }
        public float ChaseSpeed { get; set; } = 12f;
        public float MaxChaseRange { get; set; } = 800f;
        public float IdleFollowDistance { get; set; } = 80f;
        public float IdleFollowSpeed { get; set; } = 8f;
        public float TeleportRange { get; set; } = 2000f;

        private Vector2? _currentTargetPos;
        private readonly PlayerSelector _idleSelector = new PlayerSelector();

        public ChaseModule() { }

        public ChaseModule(IMovementStyle movementStyle, ITargetSelector targetSelector, float chaseSpeed = 12f)
        {
            MovementStyle = movementStyle;
            TargetSelector = targetSelector;
            ChaseSpeed = chaseSpeed;
        }

        public void OnSpawn(Projectile projectile, Player owner, IEntitySource source)
        {
            _currentTargetPos = null;
        }

        public void Update(Projectile projectile, Player owner)
        {
            Vector2? targetPos = TargetSelector.SelectTarget(projectile, owner);

            if (targetPos.HasValue)
            {
                float distToTarget = (projectile.Center - targetPos.Value).Length();
                float distToOwner = (projectile.Center - owner.Center).Length();

                if (distToTarget > MaxChaseRange || distToOwner > TeleportRange)
                {
                    targetPos = null;
                }
            }

            if (!targetPos.HasValue)
            {
                targetPos = _idleSelector.SelectTarget(projectile, owner);
                if (targetPos.HasValue)
                {
                    float distToOwner = (projectile.Center - owner.Center).Length();
                    if (distToOwner > TeleportRange)
                    {
                        projectile.Center = targetPos.Value;
                        projectile.velocity = Vector2.Zero;
                        return;
                    }
                }

                Vector2 pos = targetPos ?? owner.Center;
                projectile.velocity = MovementStyle.CalculateVelocity(projectile, projectile.velocity, pos, IdleFollowSpeed);
            }
            else
            {
                projectile.velocity = MovementStyle.CalculateVelocity(projectile, projectile.velocity, targetPos.Value, ChaseSpeed);
            }

            MovementStyle.UpdateRotation(projectile, projectile.velocity);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}