using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 炎道散射弹幕 — 向四周散射，带指数平滑插值追踪鼠标指针。
    /// 
    /// 行为像发射的导弹一样：
    ///   初始以散射初速度飞散，同时每帧用指数平滑向鼠标方向修正速度，
    ///   产生"先散开→逐渐转大弯→最终指向鼠标"的流畅导弹轨迹。
    ///   没有硬切换，完全由指数平滑的惯性自然过渡。
    /// 
    /// 行为组合：
    /// - MouseAimBehavior: 指数平滑追踪鼠标指针（不覆盖初速度，保留散射方向）
    /// - LiquidTrailBehavior: 液态火焰拖尾（黄→红渐变）
    /// - ExplosionKillBehavior: 销毁时爆炸
    /// </summary>
    public class FireScatterProj : BaseBullet
    {
        protected override void RegisterBehaviors()
        {
            // 1. 指数平滑追踪鼠标指针
            // OnSpawn 不覆盖初速度，保留武器赋予的散射方向速度
            // 每帧通过指数平滑逐渐转向鼠标，产生导弹式大转弯效果
            Behaviors.Add(new MouseAimBehavior(speed: 8f, smoothWeight: 12f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
                EnableLight = true,
                LightColor = new Vector3(1f, 0.6f, 0.1f)
            });

            // 2. 液态火焰拖尾（黄→红渐变，散射弹幕用稍小的碎片）
            Behaviors.Add(new LiquidTrailBehavior
            {
                MaxFragments = 30,
                FragmentLife = 20,
                SizeMultiplier = 0.4f,
                SpawnInterval = 1,
                AdaptiveTargetLength = 100f,   // 更长的拖尾目标长度
                SpeedLifeExponent = 0.35f,     // 速度对存活时间影响更弱，快速移动时拖尾更明显
                MinFragmentLife = 6,           // 提高下限，防止高速时拖尾断裂
                ColorStart = new Color(255, 200, 80, 255),
                ColorEnd = new Color(255, 50, 0, 0),
                Buoyancy = 0.03f,
                AirResistance = 0.97f,
                InertiaFactor = 0.3f,
                SplashFactor = 0.1f,
                SplashAngle = 0.3f,
                RandomSpread = 0.6f,
                AutoDraw = true,
                SuppressDefaultDraw = true
            });

            // 3. 销毁时爆炸（小范围）
            Behaviors.Add(new ExplosionKillBehavior
            {
                ExplodeOnKill = true,
                KillCount = 6,
                KillSpeed = 2.5f,
                KillSizeMultiplier = 0.5f,
                KillFragmentLife = 15,
                ExplodeOnTileCollide = false,
                ColorStart = new Color(255, 200, 80, 255),
                ColorEnd = new Color(255, 50, 0, 0)
            });
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.scale = 0.8f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 3; // 可穿透 3 个敌人
            Projectile.timeLeft = 90;  // 存活 90 帧
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }
    }
}
