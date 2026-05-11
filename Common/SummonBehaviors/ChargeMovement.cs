using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.SummonBehaviors
{
    /// <summary>
    /// 冲锋移动 — 召唤物向敌人直线冲刺，命中后弹回。
    ///
    /// 行为逻辑：
    /// - 无敌人时：跟随在玩家身边
    /// - 发现敌人后：蓄力 → 高速冲锋 → 命中/穿过 → 减速折返 → 冷却 → 再次冲锋
    /// - 冲锋过程中不可转向（模拟惯性）
    ///
    /// 适用召唤物：暗影兽、骨龙、犀牛等突击型
    ///
    /// 可配置参数：
    /// - ChargeRange: 发现敌人的范围
    /// - ChargeSpeed: 冲锋速度
    /// - ChargeAcceleration: 冲锋加速度
    /// - ChargeCooldown: 冲锋冷却时间（帧）
    /// - FollowSpeed: 跟随玩家时的速度
    /// - FollowDistance: 跟随距离
    /// </summary>
    public class ChargeMovement : ISummonBehavior
    {
        public string Name => "ChargeMovement";

        public float ChargeRange { get; set; } = 600f;
        public float ChargeSpeed { get; set; } = 18f;
        public float ChargeAcceleration { get; set; } = 0.3f;
        public int ChargeCooldown { get; set; } = 60;
        public float FollowSpeed { get; set; } = 8f;
        public float FollowDistance { get; set; } = 100f;
        public float TeleportRange { get; set; } = 2000f;

        private enum ChargeState
        {
            Following,
            WindingUp,
            Charging,
            Returning,
            Cooldown
        }

        private ChargeState _state = ChargeState.Following;
        private NPC _currentTarget;
        private int _stateTimer;
        private Vector2 _chargeDirection;
        private const int WindUpTime = 20;
        private const int ReturnTime = 40;

        public ChargeMovement() { }

        public ChargeMovement(float chargeRange, float chargeSpeed = 18f)
        {
            ChargeRange = chargeRange;
            ChargeSpeed = chargeSpeed;
        }

        public void OnSpawn(Projectile projectile, Player owner, IEntitySource source)
        {
            _state = ChargeState.Following;
            _currentTarget = null;
            _stateTimer = 0;
        }

        public void Update(Projectile projectile, Player owner)
        {
            switch (_state)
            {
                case ChargeState.Following:
                    UpdateFollowing(projectile, owner);
                    break;
                case ChargeState.WindingUp:
                    UpdateWindUp(projectile);
                    break;
                case ChargeState.Charging:
                    UpdateCharging(projectile, owner);
                    break;
                case ChargeState.Returning:
                    UpdateReturning(projectile, owner);
                    break;
                case ChargeState.Cooldown:
                    UpdateCooldown(projectile, owner);
                    break;
            }
        }

        private void UpdateFollowing(Projectile projectile, Player owner)
        {
            Vector2 followPos = owner.Center;
            followPos.X -= owner.direction * FollowDistance;
            followPos.Y -= 20f;

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
                float speed = System.Math.Min(FollowSpeed, dist * 0.1f + 2f);
                projectile.velocity = Vector2.Lerp(projectile.velocity, dir * speed, 0.1f);
            }
            else
            {
                projectile.velocity *= 0.85f;
            }

            projectile.rotation = projectile.velocity.X * 0.05f;

            NPC target = SummonHelper.FindNearestEnemy(projectile.Center, ChargeRange, projectile);
            if (target != null)
            {
                _currentTarget = target;
                _state = ChargeState.WindingUp;
                _stateTimer = 0;
            }
        }

        private void UpdateWindUp(Projectile projectile)
        {
            _stateTimer++;

            if (_currentTarget == null || !_currentTarget.active)
            {
                _state = ChargeState.Following;
                _currentTarget = null;
                return;
            }

            _chargeDirection = (_currentTarget.Center - projectile.Center).SafeNormalize(Vector2.Zero);
            projectile.velocity *= 0.85f;
            projectile.rotation = _chargeDirection.ToRotation() + MathHelper.PiOver2;

            if (_stateTimer >= WindUpTime)
            {
                _state = ChargeState.Charging;
                _stateTimer = 0;
                projectile.velocity = _chargeDirection * ChargeSpeed * 0.5f;
            }
        }

        private void UpdateCharging(Projectile projectile, Player owner)
        {
            _stateTimer++;

            projectile.velocity += _chargeDirection * ChargeAcceleration;
            if (projectile.velocity.Length() > ChargeSpeed)
            {
                projectile.velocity = projectile.velocity.SafeNormalize(Vector2.Zero) * ChargeSpeed;
            }

            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            float distToOwner = (projectile.Center - owner.Center).Length();
            if (_stateTimer > 90 || distToOwner > 1500f)
            {
                _state = ChargeState.Returning;
                _stateTimer = 0;
            }
        }

        private void UpdateReturning(Projectile projectile, Player owner)
        {
            _stateTimer++;

            Vector2 toOwner = owner.Center - projectile.Center;
            Vector2 dir = toOwner.SafeNormalize(Vector2.Zero);
            projectile.velocity = Vector2.Lerp(projectile.velocity, dir * FollowSpeed * 1.5f, 0.08f);
            projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (_stateTimer >= ReturnTime || toOwner.Length() < FollowDistance)
            {
                _state = ChargeState.Cooldown;
                _stateTimer = 0;
            }
        }

        private void UpdateCooldown(Projectile projectile, Player owner)
        {
            _stateTimer++;

            Vector2 followPos = owner.Center;
            followPos.X -= owner.direction * FollowDistance;
            followPos.Y -= 20f;

            Vector2 toTarget = followPos - projectile.Center;
            float dist = toTarget.Length();

            if (dist > 30f)
            {
                Vector2 dir = toTarget.SafeNormalize(Vector2.Zero);
                float speed = System.Math.Min(FollowSpeed, dist * 0.1f + 2f);
                projectile.velocity = Vector2.Lerp(projectile.velocity, dir * speed, 0.1f);
            }
            else
            {
                projectile.velocity *= 0.85f;
            }

            projectile.rotation = projectile.velocity.X * 0.05f;

            if (_stateTimer >= ChargeCooldown)
            {
                _state = ChargeState.Following;
                _stateTimer = 0;
                _currentTarget = null;
            }
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (_state == ChargeState.Charging && target == _currentTarget)
            {
                _state = ChargeState.Returning;
                _stateTimer = 0;
            }
        }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}