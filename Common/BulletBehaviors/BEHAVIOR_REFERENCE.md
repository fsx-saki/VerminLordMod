# Behavior 系统完整参考

> 位置：`Common/BulletBehaviors/`
> 基类：`BaseBullet`（继承 `ModProjectile`）
> 接口：`IBulletBehavior`
> 设计模式：**组合模式** — 一个弹幕可同时拥有多个行为

---

## 一、飞行运动行为（移动/AI 核心）

| 行为 | 类名 | 作用 | 关键参数 |
|------|------|------|----------|
| **自机狙** | `AimBehavior` | 沿初始方向直线飞行，自动旋转 | `Speed`, `AutoRotate`, `RotationOffset`, `EnableLight` |
| **追踪敌人** | `HomingBehavior` | 指数平滑追踪最近敌人 | `Speed`, `TrackingWeight`(越小越灵敏), `Range`(0=无限), `LockedTarget` |
| **追踪鼠标** | `MouseAimBehavior` | 指数平滑追踪鼠标，适合引导弹 | `Speed`, `SmoothWeight`(越大越平滑) |
| **追踪友方** | `FriendlyHomingBehavior` | 追踪最近友方玩家，到达后停止 | `HomingStrength`, `MaxSpeed`, `DetectionRange`, `ArriveDistance` |
| **摆动追踪** | `SwingHomingBehavior` | 正弦摆动 + 弧线追踪鼠标，像水蛇/游鱼 | `SwingAmplitude`, `SwingFrequency`, `TrackingWeight`, `UseArcMode` |
| **波浪飞行** | `WaveBehavior` | 正弦波轨迹飞行 | `Amplitude`, `Frequency` |
| **速度阻尼** | `DampingBehavior` | 每帧按比例减速，可设最低速度阈值 | `Damping`(0~1), `MinSpeed`, `KillOnMinSpeed`, `StopOnMinSpeed` |
| **重力** | `GravityBehavior` | 每帧受重力下落 | `Acceleration`, `MaxFallSpeed`, `GravityDirection` |
| **浮动** | `BobBehavior` | 垂直/圆形正弦浮动，适合悬浮物 | `Amplitude`, `Frequency`, `BobInX` |
| **固定位置** | `StationaryBehavior` | 锁定在原地不动，适合光环/陷阱 | `LockPosition`, `PositionOffset`, `DisableTileCollide` |
| **回旋镖** | `BoomerangBehavior` | 飞出→返回玩家 | `OutwardSpeed`, `ReturnSpeed`, `OutwardFrames`, `SpinSpeed` |
| **约束转向** | `ConstrainedSteerBehavior` | 物理向心加速度追踪，圆弧轨迹 | `AccelMagnitude`, `MaxSpeed`, `ConeHalfAngle`, `TrackMouse/TrackNPC` |
| **蓄力弹幕** | `ChargeProjectileBehavior` | 固定在玩家前方蓄力→松手发射 | `MaxChargeTime`, `ChargeDistance`, `DamageMultiplier`, `FireSpeed` |
| **汇聚弹幕** | `ConvergeProjectile`（抽象类） | 飞向目标弹幕并汇聚销毁，需继承 | `TargetProjType`, `ConvergeDistance`, `MinSpeed/MaxSpeed`, `LerpFactor` |

---

## 二、碰撞/命中响应行为

| 行为 | 类名 | 作用 | 关键参数 |
|------|------|------|----------|
| **反弹** | `BounceBehavior` | 撞物块反弹，可配置次数 | `MaxBounces`, `BounceFactor`, `KillOnMaxBounces`, `OnBounce`回调 |
| **接触销毁** | `KillOnContactBehavior` | 碰到任何东西（物块/敌人）就销毁 | `KillOnTileCollide`, `KillOnHitNPC` |
| **范围伤害** | `AreaDamageBehavior` | 每帧/间隔检测范围内敌人造成伤害，适合光环 | `HitRadius`, `HitInterval`, `UseLocalNPCHitCooldown` |
| **范围爆炸** | `OnKillAoEBehavior` | 销毁时对范围内所有敌人造成伤害+debuff | `Radius`, `DamageMultiplier`, `Buffs` |
| **吸附** | `PullBehavior` | 将范围内敌人拉向弹幕中心，适合漩涡/黑洞 | `PullRange`, `PullStrength`, `TangentFactor`, `StrengthCurve` |
| **液体反应** | `LiquidReactionBehavior` | 碰到水/岩浆/微光时触发不同效果 | `EnableMerge/Vaporize/ShimmerBounce` |

---

## 三、命中特效行为

| 行为 | 类名 | 作用 | 关键参数 |
|------|------|------|----------|
| **命中粒子** | `DustOnHitBehavior` | 命中时爆 Dust 粒子 | `DustType`, `DustCount`, `SpeedMin/Max` |
| **命中洒落** | `OnHitDropletBehavior` | 命中时洒落子弹幕（小水滴） | `ChildProjectileType`, `MinCount/MaxCount` |
| **命中治疗** | `HealOnHitBehavior` | 命中按伤害比例回血 | `HealMultiplier`, `HealDustCount` |
| **接触治疗** | `HealOnContactBehavior` | 靠近友方自动治疗并销毁 | `HealAmount`, `ContactRange` |
| **治疗追踪弹** | `HealSeekerSpawnBehavior` | 命中概率生成治疗追踪弹 | `ChildProjectileType`, `SpawnChance` |
| **命中 debuff** | `DebuffOnHitBehavior` | 命中附加 buff/debuff | `Buffs`（List of (buffType, duration)） |

---

## 四、销毁特效行为

| 行为 | 类名 | 作用 | 关键参数 |
|------|------|------|----------|
| **死亡 Dust** | `DustKillBehavior` | 销毁时爆 Dust | `DustType`, `DustCount`, `DustSpeed` |
| **死亡子弹幕** | `OnKillProjectileBurstBehavior` | 销毁时向四周发射子弹幕 | `ProjectileType`, `Count`, `Speed`, `DamageMultiplier` |
| **粒子体崩解** | `ParticleBurstBehavior` | 销毁时生成大量子弹幕飞散（崩解感） | `ChildProjectileType`, `Count`, `SpeedMin/Max`, `SpawnExtraDust` |
| **统一泼溅** | `SplashBehavior` | **5种泼溅模式**：Normal/Radial/Cone/Ring/Forward | `SplashMode`, `ChildProjectileType`, `Count`, `SpeedMin/Max` |
| **法线崩解** | `NormalBurstBehavior` | 沿法线方向泼洒（水球砸中敌人效果） | `ChildProjectileType`, `SpreadAngle`, `SideAngle` |
| **液滴泼洒** | `DropletSplashBehavior` | 存活期间定时向上泼洒液滴 | `Interval`, `Count`, `SpreadX` |
| **液体爆裂** | `LiquidBurstBehavior` | 销毁时液体飞溅（Dust 实现） | `FragmentCount`, `BurstSpeed`, `ColorStart/End` |
| **酸液飞溅** | `AcidSplashBehavior` | 销毁时给周围物块涂色（酸液水渍） | `Radius`, `PaintChance`, `PaintColor`, `AddLiquid` |

---

## 五、视觉/粒子行为

| 行为 | 类名 | 作用 | 关键参数 |
|------|------|------|----------|
| **飞行拖尾** | `DustTrailBehavior` | 飞行时持续生成 Dust 拖尾 | `DustType`, `SpawnChance`, `VelocityMultiplier`, `NoGravity` |
| **发光绘制** | `GlowDrawBehavior` | 多层发光绘制叠加 | `GlowColor`, `GlowLayers`, `GlowScaleIncrement`, `CustomTexture` |
| **环境发光** | `GlowLightBehavior` | 每帧添加 Lighting 环境光 | `LightColor` |
| **渐变缩放** | `ScaleOverLifeBehavior` | 生命周期内缩放+透明度变化 | `StartScale`, `EndScale`, `AnimateAlpha` |
| **渐入渐出** | `FadeInOutBehavior` | 按阶段控制透明度 | `FadeInDuration`, `FadeOutStart` |
| **恒定旋转** | `RotateBehavior` | 每帧固定角速度自转 | `RotationSpeed`, `OverrideAutoRotate` |
| **法阵粒子** | `FormationParticleBehavior` | 多层旋转粒子环（外圈+内圈+符文+光晕+气泡） | `OuterRingCount`, `InnerRingCount`, `RotationSpeed`, `DustType` |
| **漩涡粒子** | `VortexParticleBehavior` | Ring 模式或 Cloud 模式旋转粒子 | `UseCloudMode`, `CloudParticleCount`, `CloudRadius`, `CloudStreamerArms` |
| **粒子体** | `ParticleBodyBehavior` | 用粒子组成弹幕本体（不依赖贴图） | `ParticleCount`, `BodyRadius`, `SwirlSpeed`, `StretchOnMove` |
| **波浪粒子体** | `WaveBodyBehavior` | 二维网格粒子模拟水波扩散 | `WaveLength`, `Amplitude`, `Width`, `Rows`, `ParticlesPerRow` |
| **周期性 Dust** | `PeriodicDustBehavior` | 每帧按概率生成周围 Dust | `SpawnChance`, `DustType`, `Color` |
| **抑制绘制** | `SuppressDrawBehavior` | 阻止贴图绘制（只用粒子） | 无参数 |

---

## 六、生成布置行为

| 行为 | 类名 | 作用 | 关键参数 |
|------|------|------|----------|
| **随机散布** | `RandomSpawnBehavior` | 生成时随机偏移位置/角度/速度 | `SpreadRadius`, `AngleSpread`, `SpeedVariation` |
| **区域生成** | `RegionSpawnBehavior` | 在 Ring/Circle/Rect/Sector 形状内随机初始位置 | `Shape`, `InnerRadius`, `OuterRadius`, `InitialSpeed` |

---

## 七、辅助类

| 类名 | 作用 |
|------|------|
| `CircleSpawnHelper` | 6种法阵生成模式：汇聚/环状汇聚/追踪/切线甩出/爆发/螺旋 |
| `ExplosionSpawnHelper` | 向四面八方生成子弹幕的静态方法 |
| `CircleArrayDrawer` | 多功能法阵绘制器（双同心圆+光晕+光照+粒子环绕） |

---

## 八、组合套路速查

```csharp
// 基础直线弹
Behaviors.Add(new AimBehavior(10f));
Behaviors.Add(new DustTrailBehavior(DustID.YellowStarDust));
Behaviors.Add(new BounceBehavior(maxBounces: 3));

// 带速度阻尼的投掷物（先快后慢）
Behaviors.Add(new AimBehavior(12f));
Behaviors.Add(new DampingBehavior(0.97f));
Behaviors.Add(new GravityBehavior(0.15f));

// 追踪弹
Behaviors.Add(new HomingBehavior(speed: 10f, trackingWeight: 1f/15f));
Behaviors.Add(new GlowDrawBehavior { GlowColor = Color.Red });
Behaviors.Add(new DebuffOnHitBehavior(BuffID.OnFire, 120));

// 鼠标引导弹（先散开再转大弯）
Behaviors.Add(new RandomSpawnBehavior(spreadRadius: 30f, angleSpread: 0.2f));
Behaviors.Add(new MouseAimBehavior(speed: 8f, smoothWeight: 8f));

// 水球弹（砸中沿法线泼洒）
Behaviors.Add(new AimBehavior(12f));
Behaviors.Add(new GravityBehavior(0.3f));
Behaviors.Add(new NormalBurstBehavior { ChildProjectileType = ..., Count = 16 });
Behaviors.Add(new LiquidBurstBehavior { FragmentCount = 20, BurstSpeed = 6f });

// 法阵/光环（固定位置+范围伤害）
Behaviors.Add(new StationaryBehavior());
Behaviors.Add(new AreaDamageBehavior(hitRadius: 60f, hitInterval: 15));
Behaviors.Add(new FormationParticleBehavior { OuterRingCount = 8, OuterRingRadius = 70f });

// 纯粒子体弹幕（不显示贴图）
Behaviors.Add(new ParticleBodyBehavior { ParticleCount = 30, BodyRadius = 20f });
Behaviors.Add(new SuppressDrawBehavior());
Behaviors.Add(new ScaleOverLifeBehavior(startScale: 0.5f, endScale: 1.5f));

// 摇摆追踪弹（蛇形）
Behaviors.Add(new SwingHomingBehavior(speed: 8f, swingAmplitude: 0.15f));
Behaviors.Add(new PeriodicDustBehavior(DustID.Water, Color.LightBlue));

// 黑洞/漩涡
Behaviors.Add(new StationaryBehavior());
Behaviors.Add(new PullBehavior(pullRange: 200f, pullStrength: 0.25f));
Behaviors.Add(new VortexParticleBehavior { UseCloudMode = true, CloudParticleCount = 30 });
Behaviors.Add(new AreaDamageBehavior(hitRadius: 60f, hitInterval: 10));

// 蓄力弹
Behaviors.Add(new ChargeProjectileBehavior { MaxChargeTime = 300, DamageMultiplier = 3f });
Behaviors.Add(new ScaleOverLifeBehavior(startScale: 0.3f, endScale: 1.5f));
```

---

## 九、快速创建新弹幕模板

```csharp
using VerminLordMod.Common.BulletBehaviors;

namespace VerminLordMod.Content.Projectiles
{
    public class MyNewProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // ===== 在这里添加行为 =====
            Behaviors.Add(new AimBehavior(10f));
            // Behaviors.Add(new HomingBehavior(speed: 10f));
            // Behaviors.Add(new GravityBehavior(0.3f));
            // ...
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.timeLeft = 120;
            Projectile.penetrate = 1;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.aiStyle = -1;
        }
    }
}
```
