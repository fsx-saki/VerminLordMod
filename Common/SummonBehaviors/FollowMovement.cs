using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.SummonBehaviors
{
    /// <summary>
    /// 跟随移动 — 召唤物跟随在玩家身后。
    ///
    /// 行为逻辑：
    /// - 无敌人时：跟随在玩家身后指定距离
    /// - 有敌人进入 AggroRange 时：飞向敌人攻击
    /// - 敌人死亡/超出范围后：返回跟随位置
    /// - 距离玩家太远时：快速飞回（或传送）
    ///
    /// 适用召唤物：火灵、暗影兽、小狗等跟随型
    ///
    /// 可配置参数：
    /// - FollowDistance: 跟随距离
    /// - FollowBehind: 是否在玩家身后（根据朝向）
    /// - FollowSpeed: 跟随速度
    /// - AggroRange: 主动攻击范围
    /// - AttackSpeed: 攻击时速度
    /// - TeleportRange: 超过此距离直接传送
    /// </summary>
    public class FollowMovement : ISummonBehavior
    {
        public string Name => "FollowMovement";

        public float FollowDistance { get; set; } = 80f;
        public bool FollowBehind { get; set; } = true;
        public float FollowSpeed { get; set; } = 10f;
        public float AggroRange { get; set; } = 500f;
        public float AttackSpeed { get; set; } = 14f;
        public float TeleportRange { get; set; } = 2000f;

        private NPC _currentTarget;
        private bool _isAttacking;

        public FollowMovement() { }

        public FollowMovement(float followDistance, float aggroRange = 500f)
        {
            FollowDistance = followDistance;
            AggroRange = aggroRange;
        }

        public void OnSpawn(Projectile projectile, Player owner, IEntitySource source)
        {
            _currentTarget = null;
            _isAttacking = false;
        }

        public void Update(Projectile projectile, Player owner)
        {
            if (_isAttacking)
            {
                UpdateAttack(projectile, owner);
            }
            else
            {
                UpdateFollow(projectile, owner);
            }
        }

        private void UpdateFollow(Projectile projectile, Player owner)
        {
            Vector2 followPos = owner.Center;

            if (FollowBehind)
            {
                followPos.X -= owner.direction * FollowDistance;
                followPos.Y -= 20f;
            }
            else
            {
                followPos.Y -= FollowDistance;
            }

            float distToOwner = (projectile.Center - owner.Center).Length();

            if (distToOwner > TeleportRange)
            {
                projectile.Center = followPos;
                projectile.velocity = Vector2.Zero;
                return;
            }

            Vector2 toTarget = followPos - projectile.Center;
            float dist = toTarget.Length();

            if (dist > 30f)
            {
                Vector2 dir = toTarget.SafeNormalize(Vector2.Zero);
                float speed = System.Math.Min(FollowSpeed, dist * 0.15f + 3f);
                projectile.velocity = Vector2.Lerp(projectile.velocity, dir * speed, 0.12f);
            }
            else
            {
                projectile.velocity *= 0.85f;
            }

            projectile.rotation = projectile.velocity.X * 0.05f;

            NPC target = SummonHelper.FindNearestEnemy(projectile.Center, AggroRange, projectile);
            if (target != null)
            {
                _currentTarget = target;
                _isAttacking = true;
            }
        }

        private void UpdateAttack(Projectile projectile, Player owner)
        {
            if (_currentTarget == null || !_currentTarget.active)
            {
                _isAttacking = false;
                _currentTarget = null;
                return;
            }

            float distToTarget = (projectile.Center - _currentTarget.Center).Length();
            float distToOwner = (projectile.Center - owner.Center).Length();

            if (distToTarget > AggroRange * 1.5f || distToOwner > 1500f)
            {
                _isAttacking = false;
                _currentTarget = null;
                return;
            }

            Vector2 toTarget = _currentTarget.Center - projectile.Center;
            Vector2 dir = toTarget.SafeNormalize(Vector2.Zero);
            projectile.velocity = Vector2.Lerp(projectile.velocity, dir * AttackSpeed, 0.1f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (_isAttacking && target == _currentTarget)
            {
                _isAttacking = false;
                _currentTarget = null;
            }
        }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}