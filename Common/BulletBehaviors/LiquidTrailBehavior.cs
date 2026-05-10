using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using VerminLordMod.Content.Trails;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 液态火焰拖尾行为 — 将 TrailManager + LiquidTrail 封装为 IBulletBehavior。
    /// 
    /// 自动管理 LiquidTrail 的创建、更新、绘制和清理生命周期。
    /// 支持完整的 LiquidTrail 参数配置。
    /// 
    /// 使用示例：
    ///   Behaviors.Add(new LiquidTrailBehavior(
    ///       texture: ModContent.Request<Texture2D>("VerminLordMod/Content/Projectiles/StarfireProj").Value,
    ///       colorStart: new Color(255, 220, 100, 255),
    ///       colorEnd: new Color(255, 30, 0, 0))
    ///   {
    ///       MaxFragments = 50,
    ///       FragmentLife = 15,
    ///       SizeMultiplier = 0.6f,
    ///       SpawnInterval = 1,
    ///       Buoyancy = 0.05f,
    ///       AirResistance = 0.96f,
    ///       InertiaFactor = 0.4f,
    ///       SplashFactor = 0.2f,
    ///       SplashAngle = 0.5f,
    ///       RandomSpread = 0.8f
    ///   });
    /// </summary>
    public class LiquidTrailBehavior : IBulletBehavior
    {
        public string Name => "LiquidTrail";

        /// <summary>拖尾管理器实例</summary>
        public TrailManager TrailManager { get; } = new TrailManager();

        /// <summary>LiquidTrail 实例（运行时只读）</summary>
        public LiquidTrail Trail { get; private set; }

        // ===== 可配置参数 =====

        /// <summary>拖尾贴图（null 则使用弹幕默认贴图）</summary>
        public Texture2D TrailTexture { get; set; }

        /// <summary>碎片起始颜色（年轻）</summary>
        public Color ColorStart { get; set; } = new Color(255, 220, 100, 255);

        /// <summary>碎片结束颜色（年老）</summary>
        public Color ColorEnd { get; set; } = new Color(255, 30, 0, 0);

        /// <summary>最大碎片数量</summary>
        public int MaxFragments { get; set; } = 40;

        /// <summary>碎片存活时间（帧）</summary>
        public int FragmentLife { get; set; } = 30;

        /// <summary>碎片大小倍率</summary>
        public float SizeMultiplier { get; set; } = 1f;

        /// <summary>基础生成间隔（帧），速度慢时的间隔</summary>
        public int SpawnInterval { get; set; } = 2;

        /// <summary>是否启用速度自适应密度（速度快时自动增加碎片密度）</summary>
        public bool AdaptiveDensity { get; set; } = true;

        /// <summary>速度自适应密度阈值下限</summary>
        public float AdaptiveSpeedThreshold { get; set; } = 4f;

        /// <summary>速度自适应密度系数（每达到此速度值每帧多生成一个碎片）</summary>
        public float AdaptiveDensityFactor { get; set; } = 6f;

        /// <summary>是否启用速度自适应存活时间（速度快时碎片更快消失，保持拖尾长度恒定）</summary>
        public bool AdaptiveLife { get; set; } = true;
      
        /// <summary>拖尾目标长度（像素），自适应存活时间模式下拖尾长度 ≈ 此值</summary>
        public float AdaptiveTargetLength { get; set; } = 60f;
      
        /// <summary>
        /// 速度对存活时间的影响指数（0~1）。
        /// 0 = 速度不影响存活时间（完全使用 FragmentLife）
        /// 1 = 完全线性影响（currentLife = targetLength / speed）
        /// 推荐值 0.3~0.5，让快速移动时拖尾变长但不至于断裂。
        /// </summary>
        public float SpeedLifeExponent { get; set; } = 0.4f;
      
        /// <summary>碎片存活时间下限（帧）</summary>
        public int MinFragmentLife { get; set; } = 4;

        /// <summary>空气阻力系数</summary>
        public float AirResistance { get; set; } = 0.92f;

        /// <summary>浮力系数（正值向上）</summary>
        public float Buoyancy { get; set; } = 0.08f;

        /// <summary>反向惯性系数</summary>
        public float InertiaFactor { get; set; } = 0.25f;

        /// <summary>侧向飞溅系数</summary>
        public float SplashFactor { get; set; } = 0.3f;

        /// <summary>侧向飞溅角度范围</summary>
        public float SplashAngle { get; set; } = 0.8f;

        /// <summary>随机扩散范围</summary>
        public float RandomSpread { get; set; } = 1.2f;

        /// <summary>生成位置偏移</summary>
        public Vector2 SpawnOffset { get; set; } = Vector2.Zero;

        /// <summary>
        /// 是否在 PreDraw 中由本行为绘制拖尾。
        /// 如果为 false，需要外部自行调用 TrailManager.Draw()。
        /// </summary>
        public bool AutoDraw { get; set; } = true;

        /// <summary>
        /// 是否在 PreDraw 中返回 false（阻止引擎默认绘制）。
        /// 当弹幕有自定义绘制（如发光层）时需要设为 true。
        /// </summary>
        public bool SuppressDefaultDraw { get; set; } = false;

        public LiquidTrailBehavior() { }

        /// <summary>
        /// 使用贴图快速构造（颜色使用默认星火风格）
        /// </summary>
        public LiquidTrailBehavior(Texture2D texture)
        {
            TrailTexture = texture;
        }

        /// <summary>
        /// 使用贴图和颜色快速构造
        /// </summary>
        public LiquidTrailBehavior(Texture2D texture, Color colorStart, Color colorEnd)
        {
            TrailTexture = texture;
            ColorStart = colorStart;
            ColorEnd = colorEnd;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            // 如果未指定贴图，使用弹幕默认贴图
            Texture2D tex = TrailTexture;
            if (tex == null)
            {
                tex = Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
            }

            // 创建并配置 LiquidTrail
            Trail = new LiquidTrail
            {
            	TrailTexture = tex,
            	ColorStart = ColorStart,
            	ColorEnd = ColorEnd,
            	MaxFragments = MaxFragments,
            	FragmentLife = FragmentLife,
            	SizeMultiplier = SizeMultiplier,
            	SpawnInterval = SpawnInterval,
            	AdaptiveDensity = AdaptiveDensity,
            	AdaptiveSpeedThreshold = AdaptiveSpeedThreshold,
            	AdaptiveDensityFactor = AdaptiveDensityFactor,
            	AdaptiveLife = AdaptiveLife,
            	AdaptiveTargetLength = AdaptiveTargetLength,
            	SpeedLifeExponent = SpeedLifeExponent,
            	MinFragmentLife = MinFragmentLife,
            	AirResistance = AirResistance,
            	Buoyancy = Buoyancy,
            	InertiaFactor = InertiaFactor,
            	SplashFactor = SplashFactor,
            	SplashAngle = SplashAngle,
            	RandomSpread = RandomSpread,
            	SpawnOffset = SpawnOffset
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

        bool? IBulletBehavior.OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            return null;
        }
    }
}
