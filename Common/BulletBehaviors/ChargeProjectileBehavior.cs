using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 蓄力弹幕行为 — 将"弹幕固定在玩家前方蓄力，松手后发射"的通用逻辑解耦为独立行为。
    /// 
    /// 核心机制：
    /// - 蓄力阶段：弹幕固定在玩家前方，跟随鼠标方向旋转
    /// - 弹幕逐渐变大（scale），透明度降低（alpha），伤害增加
    /// - 支持自定义粒子效果回调
    /// - 松手后设置 _hasFired = true，后续行为接管运动
    /// - 通过 ai[2] = RoundCompleteFlag 通知武器端"本轮完成"
    /// 
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new ChargeProjectileBehavior
    /// {
    ///     MaxChargeTime = 300,           // 5秒最大蓄力
    ///     ChargeDistance = 120f,          // 蓄力时距离玩家的距离
    ///     StartScale = 0.3f,             // 初始缩放
    ///     EndScale = 1.5f,               // 满蓄力缩放
    ///     StartAlpha = 80,               // 初始透明度
    ///     EndAlpha = 0,                  // 满蓄力透明度
    ///     DamageMultiplier = 3f,         // 满蓄力伤害倍率（额外 +200%）
    ///     OnChargeParticle = (proj, progress) => { /* 自定义粒子 */ },
    ///     OnChargeUpdate = (proj, progress) => { /* 自定义每帧逻辑 */ }
    /// });
    /// </code>
    /// </summary>
    public class ChargeProjectileBehavior : IBulletBehavior
    {
        public string Name => "ChargeProjectile";

        // ===== 蓄力参数 =====

        /// <summary>最大蓄力时间（帧），默认 300（5秒）</summary>
        public int MaxChargeTime { get; set; } = 300;

        /// <summary>蓄力时弹幕距离玩家的基础距离</summary>
        public float ChargeDistance { get; set; } = 120f;

        /// <summary>初始缩放</summary>
        public float StartScale { get; set; } = 0.3f;

        /// <summary>满蓄力缩放</summary>
        public float EndScale { get; set; } = 1.5f;

        /// <summary>初始透明度（0~255）</summary>
        public int StartAlpha { get; set; } = 80;

        /// <summary>满蓄力透明度</summary>
        public int EndAlpha { get; set; } = 0;

        /// <summary>
        /// 满蓄力伤害倍率。
        /// 最终伤害 = baseDamage + baseDamage * chargeProgress * DamageMultiplier
        /// 例如 DamageMultiplier = 2f，满蓄力时伤害变为 3 倍（base + base*2）
        /// </summary>
        public float DamageMultiplier { get; set; } = 2f;

        /// <summary>蓄力时弹幕旋转速度（弧度/帧）</summary>
        public float ChargeRotationSpeed { get; set; } = 0.02f;

        /// <summary>
        /// 弹幕推出后，通过 ai[2] 通知武器端"本轮完成"的标志值。
        /// 与 ChargeWeaponTemplate.RoundCompleteFlag 对应。
        /// </summary>
        public float RoundCompleteFlag { get; set; } = 1f;

        /// <summary>推出弹幕的初始速度</summary>
        public float FireSpeed { get; set; } = 12f;

        // ===== 可选回调 =====

        /// <summary>蓄力阶段每帧粒子效果回调 (projectile, chargeProgress 0~1)</summary>
        public System.Action<Projectile, float> OnChargeParticle { get; set; } = null;

        /// <summary>蓄力阶段每帧额外更新回调 (projectile, chargeProgress 0~1)</summary>
        public System.Action<Projectile, float> OnChargeUpdate { get; set; } = null;

        /// <summary>推出时的回调 (projectile, chargeProgress 0~1)</summary>
        public System.Action<Projectile, float> OnFire { get; set; } = null;

        // ===== 运行时状态 =====

        /// <summary>是否已推出（蓄力结束）</summary>
        public bool HasFired { get; private set; } = false;

        /// <summary>当前蓄力时间（帧）</summary>
        public int ChargeTime { get; private set; } = 0;

        /// <summary>当前蓄力进度 0~1</summary>
        public float ChargeProgress => ChargeTime / (float)MaxChargeTime;

        /// <summary>玩家引用</summary>
        private Player _owner;

        /// <summary>基础伤害（蓄力开始时记录）</summary>
        private int _baseDamage;

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _owner = Main.player[projectile.owner];
            ChargeTime = 0;
            HasFired = false;

            _baseDamage = projectile.damage;

            // 初始透明度
            projectile.alpha = StartAlpha;

            // 计算弹幕初始位置：鼠标方向前方
            Vector2 dir = Main.MouseWorld - _owner.Center;
            if (dir == Vector2.Zero) dir = Vector2.UnitX;
            dir.Normalize();
            projectile.Center = _owner.Center + dir * ChargeDistance;

            // 存储鼠标方向用于推出（使用 ai[1]）
            projectile.ai[1] = dir.ToRotation();
        }

        public void Update(Projectile projectile)
        {
            // 检查玩家是否还活着
            if (_owner == null || !_owner.active || _owner.dead)
            {
                projectile.Kill();
                return;
            }

            // 检查玩家是否还在蓄力
            bool isCharging = _owner.channel && !HasFired;

            if (isCharging)
            {
                UpdateCharging(projectile);
            }
            else if (!HasFired)
            {
                // 松手 → 推出
                FireProjectile(projectile);
            }
        }

        /// <summary>
        /// 蓄力阶段更新
        /// </summary>
        private void UpdateCharging(Projectile projectile)
        {
            ChargeTime++;
            if (ChargeTime > MaxChargeTime)
                ChargeTime = MaxChargeTime;

            float progress = ChargeProgress;

            // 0. 持续更新鼠标方向，让弹幕跟随鼠标旋转
            Vector2 mouseDir = Main.MouseWorld - _owner.Center;
            if (mouseDir != Vector2.Zero)
            {
                mouseDir.Normalize();
                projectile.ai[1] = mouseDir.ToRotation();
            }

            // 1. 弹幕位置：跟随玩家，保持在玩家前方
            float currentDist = ChargeDistance * (0.8f + progress * 0.4f);
            float dirAngle = projectile.ai[1];
            Vector2 dir = dirAngle.ToRotationVector2();
            projectile.Center = _owner.Center + dir * currentDist;

            // 2. 透明度逐渐降低
            projectile.alpha = (int)MathHelper.Lerp(StartAlpha, EndAlpha, progress);

            // 3. 弹幕逐渐变大
            projectile.scale = MathHelper.Lerp(StartScale, EndScale, progress);

            // 4. 伤害逐渐增加
            projectile.damage = _baseDamage + (int)(_baseDamage * progress * DamageMultiplier);

            // 5. 弹幕缓慢旋转
            projectile.rotation += ChargeRotationSpeed;

            // 6. 自定义每帧更新
            OnChargeUpdate?.Invoke(projectile, progress);

            // 7. 粒子效果
            OnChargeParticle?.Invoke(projectile, progress);
        }

        /// <summary>
        /// 推出弹幕
        /// </summary>
        private void FireProjectile(Projectile projectile)
        {
            HasFired = true;

            float progress = ChargeProgress;

            // 计算推出方向（鼠标方向）
            float fireAngle = projectile.ai[1];
            Vector2 fireDir = fireAngle.ToRotationVector2();

            // 设置弹幕速度
            projectile.velocity = fireDir * FireSpeed;

            // 设置弹幕朝向
            projectile.rotation = fireAngle;

            // 通知武器端本轮完成
            projectile.ai[2] = RoundCompleteFlag;

            // 自定义推出回调
            OnFire?.Invoke(projectile, progress);
        }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft) { }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true; // 允许默认绘制
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            return null; // 不处理碰撞
        }
    }
}
