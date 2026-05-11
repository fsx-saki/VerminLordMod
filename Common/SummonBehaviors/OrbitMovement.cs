using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace VerminLordMod.Common.SummonBehaviors
{
    /// <summary>
    /// 环绕移动 — 召唤物围绕玩家做圆周运动。
    ///
    /// 行为逻辑：
    /// - 无敌人时：围绕玩家以 OrbitDistance 为半径旋转
    /// - 有敌人进入 BreakRange 时：脱离轨道，飞向敌人攻击
    /// - 敌人死亡/超出范围后：返回轨道
    ///
    /// 适用召唤物：水母、光精灵、星灵等浮游型
    ///
    /// 可配置参数：
    /// - OrbitDistance: 轨道半径
    /// - OrbitSpeed: 旋转速度（弧度/帧）
    /// - OrbitHeightOffset: 轨道高度偏移（Y轴）
    /// - BreakRange: 脱离轨道追击敌人的范围
    /// - AttackSpeed: 追击敌人时的速度
    /// - ReturnSpeed: 返回轨道时的速度
    /// </summary>
    public class OrbitMovement : ISummonBehavior
    {
        public string Name => "OrbitMovement";

        public float OrbitDistance { get; set; } = 120f;
        public float OrbitSpeed { get; set; } = 0.03f;
        public float OrbitHeightOffset { get; set; } = -30f;
        public float BreakRange { get; set; } = 400f;
        public float AttackSpeed { get; set; } = 12f;
        public float ReturnSpeed { get; set; } = 8f;
        public bool Clockwise { get; set; } = true;

        private float _orbitAngle;
        private NPC _currentTarget;
        private bool _isAttacking;

        public OrbitMovement() { }

        public OrbitMovement(float orbitDistance, float orbitSpeed = 0.03f)
        {
            OrbitDistance = orbitDistance;
            OrbitSpeed = orbitSpeed;
        }

        public void OnSpawn(Projectile projectile, Player owner, IEntitySource source)
        {
            _orbitAngle = Main.rand.NextFloat(MathHelper.TwoPi);
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
                UpdateOrbit(projectile, owner);
            }
        }

        private void UpdateOrbit(Projectile projectile, Player owner)
        {
            _orbitAngle += (Clockwise ? 1 : -1) * OrbitSpeed;
            if (_orbitAngle > MathHelper.TwoPi) _orbitAngle -= MathHelper.TwoPi;
            if (_orbitAngle < 0) _orbitAngle += MathHelper.TwoPi;

            Vector2 orbitCenter = owner.Center + new Vector2(0, OrbitHeightOffset);
            Vector2 targetPos = orbitCenter + new Vector2(
                (float)System.Math.Cos(_orbitAngle) * OrbitDistance,
                (float)System.Math.Sin(_orbitAngle) * OrbitDistance * 0.5f
            );

            Vector2 toTarget = targetPos - projectile.Center;
            float dist = toTarget.Length();

            if (dist > 20f)
            {
                Vector2 dir = toTarget.SafeNormalize(Vector2.Zero);
                float speed = System.Math.Min(ReturnSpeed, dist * 0.1f + 2f);
                projectile.velocity = Vector2.Lerp(projectile.velocity, dir * speed, 0.1f);
            }
            else
            {
                projectile.velocity *= 0.9f;
            }

            projectile.rotation = projectile.velocity.X * 0.05f;

            NPC target = SummonHelper.FindNearestEnemy(projectile.Center, BreakRange, projectile);
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

            if (distToTarget > BreakRange * 1.5f || distToOwner > 1200f)
            {
                _isAttacking = false;
                _currentTarget = null;
                return;
            }

            Vector2 toTarget = _currentTarget.Center - projectile.Center;
            Vector2 dir = toTarget.SafeNormalize(Vector2.Zero);
            projectile.velocity = Vector2.Lerp(projectile.velocity, dir * AttackSpeed, 0.08f);
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