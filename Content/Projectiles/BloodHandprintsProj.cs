using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.Buffs.AddToEnemy;
using VerminLordMod.Content.DamageClasses;
using VerminLordMod.Content.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent;

namespace VerminLordMod.Content.Projectiles
{
    /// <summary>
    /// 血手印弹幕 — 蓄力型弹幕。
    ///
    /// 蓄力阶段（_hasFired == false）：
    /// - 弹幕固定在玩家身前，跟随玩家移动
    /// - 透明度逐渐降低（alpha 80→0），伤害逐渐增加
    /// - 弹幕逐渐变大（scale 0.3→1.0），越来越清晰
    /// - 红色粒子从玩家身上四散飞开，然后平滑汇聚到弹幕
    /// - 双重同心圆法阵在玩家背后旋转（使用 CircleArrayDrawer）
    /// - 每秒扣 1 滴血（由武器端处理）
    ///
    /// 推出阶段（_hasFired == true）：
    /// - 弹幕沿鼠标方向缓缓推出
    /// - 带红色液态拖尾
    /// </summary>
    public class BloodHandprintsProj : ModProjectile
    {
        // ===== 蓄力参数 =====
        private const int MaxChargeTime = 300;      // 5秒最大蓄力
        private const float ChargeDistance = 120f;   // 蓄力时距离玩家的距离（×1.5）
        private const int ParticleInterval = 6;      // 粒子汇聚间隔（帧）

        // ===== 运行时状态 =====
        private Player _owner;
        private int _chargeTime;
        private int _particleTimer;
        private bool _hasFired;

        // ===== 法阵绘制器 =====
        private CircleArrayDrawer _circleDrawer;

        // ===== 液态拖尾 =====
        private TrailManager _trailManager;
        private LiquidTrail _liquidTrail;

        public override void SetStaticDefaults()
        {
            // 标记为弹幕类，不需要额外静态设置
        }

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.scale = 0.3f;       // 初始缩小，蓄力后逐渐变大
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 600;
            Projectile.alpha = 80;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
            Projectile.netImportant = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            _owner = Main.player[Projectile.owner];
            _chargeTime = 0;
            _particleTimer = 0;
            _hasFired = false;

            Projectile.alpha = 80;

            // 计算弹幕初始位置：鼠标方向前方
            Vector2 dir = Main.MouseWorld - _owner.Center;
            if (dir == Vector2.Zero) dir = Vector2.UnitX;
            dir.Normalize();
            Projectile.Center = _owner.Center + dir * ChargeDistance;

            // 存储鼠标方向用于推出
            Projectile.ai[1] = dir.ToRotation();

            // 初始化法阵绘制器
            _circleDrawer = new CircleArrayDrawer(
                texPath: "VerminLordMod/Content/Projectiles/BloodC",
                outerRadius: 90f, outerRadiusCharge: 60f,
                innerRadius: 60f, innerRadiusCharge: 45f,
                colorOuter: new Color(100, 0, 0, 80),
                colorInner: new Color(255, 50, 50, 180),
                rotationSpeed1: 0.03f, rotationSpeed2: -0.024f);
            _circleDrawer.Init(0f, MathHelper.PiOver2);

            // 初始化液态拖尾
            _trailManager = new TrailManager();
            Texture2D tex = ModContent.Request<Texture2D>(
                "VerminLordMod/Content/Projectiles/BloodHandprintsProj").Value;
            _liquidTrail = _trailManager.AddLiquidTrail(tex,
                colorStart: new Color(180, 20, 20, 200),
                colorEnd: new Color(80, 0, 0, 0),
                maxFragments: 20,
                fragmentLife: 20,
                sizeMultiplier: 1.2f,
                spawnInterval: 1);
            _liquidTrail.Buoyancy = -0.02f;
            _liquidTrail.AirResistance = 0.95f;
            _liquidTrail.InertiaFactor = 0.3f;
            _liquidTrail.SplashFactor = 0.4f;
            _liquidTrail.SplashAngle = 0.5f;
            _liquidTrail.RandomSpread = 0.6f;
        }

        public override void AI()
        {
            // 检查玩家是否还活着
            if (_owner == null || !_owner.active || _owner.dead)
            {
                Projectile.Kill();
                return;
            }

            // 检查玩家是否还在蓄力
            bool isCharging = _owner.channel && !_hasFired;

            if (isCharging)
            {
                UpdateCharging();
            }
            else if (!_hasFired)
            {
                // 松手 → 推出
                FireProjectile();
            }

            // 如果已经推出，正常飞行
            if (_hasFired)
            {
                UpdateFlying();
            }

            // 更新拖尾（飞行阶段才显示拖尾）
            if (_hasFired && _trailManager != null)
            {
                _trailManager.Update(Projectile.Center, Projectile.velocity);
            }
        }

        /// <summary>
        /// 蓄力阶段更新
        /// </summary>
        private void UpdateCharging()
        {
            _chargeTime++;
            if (_chargeTime > MaxChargeTime)
                _chargeTime = MaxChargeTime;

            float chargeProgress = Math.Min(_chargeTime / (float)MaxChargeTime, 1f);

            // 0. 持续更新鼠标方向，让弹幕跟随鼠标旋转
            Vector2 mouseDir = Main.MouseWorld - _owner.Center;
            if (mouseDir != Vector2.Zero)
            {
                mouseDir.Normalize();
                Projectile.ai[1] = mouseDir.ToRotation();
            }

            // 1. 弹幕位置：跟随玩家，保持在玩家前方（蓄力越久距离越近，凝聚感）
            float currentDist = ChargeDistance * (0.8f + chargeProgress * 0.4f);
            float dirAngle = Projectile.ai[1];
            Vector2 dir = dirAngle.ToRotationVector2();
            Projectile.Center = _owner.Center + dir * currentDist;

            // 2. 透明度逐渐降低（从 80 降到 0）
            Projectile.alpha = (int)(80 * (1f - chargeProgress));

            // 3. 弹幕逐渐变大（从 0.3 到 1.0）
            Projectile.scale = MathHelper.Lerp(0.3f, 1.0f, chargeProgress);

            // 4. 伤害逐渐增加
            int baseDamage = Projectile.originalDamage;
            Projectile.damage = baseDamage + (int)(baseDamage * chargeProgress * 2f);

            // 5. 弹幕缓慢旋转
            Projectile.rotation += 0.02f;

            // 6. 法阵旋转
            _circleDrawer.Update();

            // 7. 红色粒子效果
            _particleTimer++;
            if (_particleTimer % ParticleInterval == 0)
            {
                SpawnChargeParticles(chargeProgress);
            }

            // 8. 玩家身上持续散发红色粒子（四散飞开）
            if (_particleTimer % 3 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 scatterVel = Main.rand.NextVector2Circular(6f, 6f);
                    Dust d = Dust.NewDustPerfect(
                        _owner.Center + Main.rand.NextVector2Circular(20f, 20f),
                        DustID.Blood,
                        scatterVel, 100, Color.Red, Main.rand.NextFloat(1f, 1.8f)
                    );
                    d.noGravity = true;
                    d.fadeIn = 0.3f;
                }
            }

            // 9. 弹幕自身发光粒子
            if (_particleTimer % 2 == 0)
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(8f, 8f),
                    DustID.Blood,
                    Main.rand.NextVector2Circular(1f, 1f),
                    50, Color.Red, Main.rand.NextFloat(1.2f, 2f)
                );
                d.noGravity = true;
            }

            // 蓄力时 velocity 为零
            Projectile.velocity = Vector2.Zero;
        }

        /// <summary>
        /// 生成汇聚效果 — 从玩家身上发射 BloodWaterProj 飞向血手印弹幕
        ///
        /// 三种效果：
        /// - 散射 Dust（短命）：快速四散飞开，模拟血液迸发
        /// - BloodWaterProj 弹幕（长命）：带液态血液拖尾，弧线飞向血手印
        /// - 弹幕周围旋转粒子环：强化凝结感
        /// </summary>
        private void SpawnChargeParticles(float chargeProgress)
        {
            Vector2 playerCenter = _owner.Center;
            Vector2 toProj = Projectile.Center - playerCenter;
            float baseAngle = toProj.ToRotation();

            // 汇聚角度：蓄力初期大角度散开，后期收窄
            float spreadAngle = MathHelper.Lerp(MathHelper.Pi * 0.67f, MathHelper.PiOver2 * 0.33f, chargeProgress);

            // ===== 1. 散射 Dust（短命、快速飞散） =====
            int scatterCount = 1 + (int)(chargeProgress * 2);
            for (int i = 0; i < scatterCount; i++)
            {
                Vector2 startPos = playerCenter + Main.rand.NextVector2Circular(20f, 20f);
                float angle = baseAngle + Main.rand.NextFloat(-MathHelper.Pi, MathHelper.Pi);
                float speed = Main.rand.NextFloat(4f, 10f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                Dust d = Dust.NewDustPerfect(
                    startPos, DustID.Blood, vel,
                    80, Color.DarkRed, Main.rand.NextFloat(0.6f, 1.0f)
                );
                d.noGravity = true;
            }

            // ===== 2. BloodWaterProj 弹幕汇聚（带液态拖尾） =====
            int convergeCount = 1 + (int)(chargeProgress * 2);
            for (int i = 0; i < convergeCount; i++)
            {
                Vector2 spawnPos = playerCenter + Main.rand.NextVector2Circular(10f, 10f);
                float angleOffset = Main.rand.NextFloat(-spreadAngle, spreadAngle);
                float angle = baseAngle + angleOffset;
                float speed = Main.rand.NextFloat(3f, 7f) * (0.8f + chargeProgress * 0.3f);
                Vector2 vel = angle.ToRotationVector2() * speed;

                int proj = Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    spawnPos, vel,
                    ModContent.ProjectileType<BloodWaterProj>(),
                    0, 0f, Projectile.owner
                );

                if (proj >= 0 && proj < Main.maxProjectiles)
                {
                    var bloodWater = Main.projectile[proj].ModProjectile as BloodWaterProj;
                    if (bloodWater != null)
                    {
                        bloodWater.TargetProjIndex = Projectile.whoAmI;
                    }
                }
            }

            // ===== 3. 蓄力后期：弹幕周围持续旋转的粒子环 =====
            if (chargeProgress > 0.3f)
            {
                int ringCount = (int)(chargeProgress * 4);
                for (int i = 0; i < ringCount; i++)
                {
                    float ringAngle = _circleDrawer.CurrentRotation1 + MathHelper.TwoPi * i / ringCount;
                    float ringRadius = 20f + chargeProgress * 20f;
                    Vector2 ringPos = Projectile.Center + ringAngle.ToRotationVector2() * ringRadius;

                    Dust d = Dust.NewDustPerfect(
                        ringPos, DustID.Blood, Vector2.Zero,
                        30, Color.Red, Main.rand.NextFloat(0.8f, 1.5f)
                    );
                    d.noGravity = true;
                }
            }
        }

        /// <summary>
        /// 推出弹幕
        /// </summary>
        private void FireProjectile()
        {
            _hasFired = true;

            // 通知武器端：弹幕已推出，本轮完成
            Projectile.ai[2] = 1f;

            float chargeProgress = Math.Min(_chargeTime / (float)MaxChargeTime, 1f);
            float fireSpeed = MathHelper.Lerp(6f, 16f, chargeProgress);

            float dirAngle = Projectile.ai[1];
            Projectile.velocity = dirAngle.ToRotationVector2() * fireSpeed;
            Projectile.rotation = dirAngle + MathHelper.PiOver2;
            Projectile.timeLeft = 120;

            // 推出爆发粒子
            ExplosionSpawnHelper.SpawnBurstDust(
                Projectile.Center,
                dustType: DustID.Blood,
                count: 20,
                speedMin: 3f, speedMax: 8f,
                colorStart: new Color(180, 20, 20, 200),
                colorEnd: new Color(80, 0, 0, 0),
                scaleMin: 1f, scaleMax: 2.5f);
        }

        /// <summary>
        /// 飞行阶段更新
        /// </summary>
        private void UpdateFlying()
        {
            Projectile.velocity *= 0.98f;

            if (Projectile.velocity.Length() < 1f)
            {
                Projectile.Kill();
                return;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // 飞行粒子
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(
                    Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    DustID.Blood,
                    -Projectile.velocity * 0.3f + Main.rand.NextVector2Circular(1f, 1f),
                    50, Color.Red, Main.rand.NextFloat(0.8f, 1.5f)
                );
                d.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;

            if (!_hasFired)
            {
                // 蓄力阶段：使用 CircleArrayDrawer 绘制双重同心圆法阵
                float chargeProgress = Math.Min(_chargeTime / (float)MaxChargeTime, 1f);
                _circleDrawer.Draw(sb, _owner.Center, chargeProgress);
            }

            // 绘制液态拖尾（飞行阶段）
            if (_hasFired && _trailManager != null)
            {
                _trailManager.Draw(sb);
            }

            // 绘制弹幕本体
            Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = tex.Size() * 0.5f;
            Color drawColor = Projectile.GetAlpha(lightColor);

            sb.Draw(tex, Projectile.Center - Main.screenPosition, null,
                drawColor, Projectile.rotation, origin,
                Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<BloodHandprintsbuff>(), 200);
        }

        public override void OnKill(int timeLeft)
        {
            // 1. 产生大量 BloodDropProj 向四面八方飞散（使用 ExplosionSpawnHelper）
            ExplosionSpawnHelper.SpawnProjectiles(
                Projectile.Center,
                ModContent.ProjectileType<BloodDropProj>(),
                count: 8 + Main.rand.Next(5), // 8~12 个
                speedMin: 4f, speedMax: 10f,
                owner: Projectile.owner,
                spreadRadius: 10f,
                angleSpread: 0.3f,
                source: Projectile.GetSource_FromThis());

            // 2. 血液飞溅粒子（Dust 爆炸效果）
            ExplosionSpawnHelper.SpawnBurstDust(
                Projectile.Center,
                dustType: DustID.Blood,
                count: 20,
                speedMin: 3f, speedMax: 8f,
                colorStart: new Color(180, 20, 20, 200),
                colorEnd: new Color(80, 0, 0, 0),
                scaleMin: 0.6f, scaleMax: 1.5f);

            // 3. 血雾粒子
            ExplosionSpawnHelper.SpawnMistDust(
                Projectile.Center,
                dustType: DustID.Blood,
                count: 10,
                speedMin: 1f, speedMax: 4f,
                color: new Color(100, 0, 0, 80),
                scaleMin: 0.3f, scaleMax: 0.6f);
        }
    }
}
