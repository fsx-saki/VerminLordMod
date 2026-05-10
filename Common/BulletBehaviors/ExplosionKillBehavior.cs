using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Content;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 爆炸效果行为 — 在弹幕生命周期各阶段生成 LiquidTrailManager 爆炸效果。
    /// 
    /// 支持两种触发方式（可同时启用）：
    /// - OnKill：弹幕销毁时爆炸
    /// - OnTileCollide：碰撞物块时爆炸（可配合 BounceBehavior 在最大反弹次数后触发）
    /// 
    /// 爆炸效果由 LiquidTrailManager 的静态 UpdateAll/DrawAll 统一管理生命周期，
    /// 不受 projectile 生命周期影响。
    /// 
    /// 使用示例（星火弹风格）：
    ///   // OnKill 爆炸（timeout 时）
    ///   Behaviors.Add(new ExplosionKillBehavior
    ///   {
    ///       ExplodeOnKill = true,
    ///       KillCount = 15, KillSpeed = 4f, KillSizeMultiplier = 1f, KillFragmentLife = 25,
    ///       // OnTileCollide 爆炸（反弹耗尽时）
    ///       ExplodeOnTileCollide = true,
    ///       TileCollideCount = 12, TileCollideSpeed = 3f, TileCollideSizeMultiplier = 0.8f, TileCollideFragmentLife = 20,
    ///       // 共用颜色/贴图
    ///       ColorStart = new Color(255, 220, 100, 255),
    ///       ColorEnd = new Color(255, 30, 0, 0)
    ///   });
    /// </summary>
    public class ExplosionKillBehavior : IBulletBehavior
    {
        public string Name => "ExplosionKill";

        // ===== 通用参数 =====

        /// <summary>拖贴图（null 则使用弹幕默认贴图）</summary>
        public Texture2D TrailTexture { get; set; } = null;

        /// <summary>爆炸专用贴图（null 则使用 TrailTexture）</summary>
        public Texture2D ExplosionTexture { get; set; } = null;

        /// <summary>碎片起始颜色</summary>
        public Color ColorStart { get; set; } = new Color(255, 220, 100, 255);

        /// <summary>碎片结束颜色</summary>
        public Color ColorEnd { get; set; } = new Color(255, 30, 0, 0);

        // ===== OnKill 爆炸参数 =====

        /// <summary>是否在 OnKill 时生成爆炸</summary>
        public bool ExplodeOnKill { get; set; } = true;

        /// <summary>OnKill 爆炸碎片数量</summary>
        public int KillCount { get; set; } = 15;

        /// <summary>OnKill 爆炸碎片飞溅速度</summary>
        public float KillSpeed { get; set; } = 4f;

        /// <summary>OnKill 爆炸碎片大小倍率</summary>
        public float KillSizeMultiplier { get; set; } = 1f;

        /// <summary>OnKill 爆炸碎片存活时间（帧），null 则使用 KillDefaultFragmentLife</summary>
        public int? KillFragmentLife { get; set; } = null;

        /// <summary>OnKill 爆炸默认碎片存活时间（当 KillFragmentLife 为 null 时使用）</summary>
        public int KillDefaultFragmentLife { get; set; } = 25;

        /// <summary>OnKill 爆炸碎片跳过概率（0~1）</summary>
        public float KillSkipChance { get; set; } = 0f;

        // ===== OnTileCollide 爆炸参数 =====

        /// <summary>是否在 OnTileCollide 时生成爆炸</summary>
        public bool ExplodeOnTileCollide { get; set; } = false;

        /// <summary>OnTileCollide 爆炸碎片数量</summary>
        public int TileCollideCount { get; set; } = 12;

        /// <summary>OnTileCollide 爆炸碎片飞溅速度</summary>
        public float TileCollideSpeed { get; set; } = 3f;

        /// <summary>OnTileCollide 爆炸碎片大小倍率</summary>
        public float TileCollideSizeMultiplier { get; set; } = 0.8f;

        /// <summary>OnTileCollide 爆炸碎片存活时间（帧），null 则使用 TileCollideDefaultFragmentLife</summary>
        public int? TileCollideFragmentLife { get; set; } = null;

        /// <summary>OnTileCollide 爆炸默认碎片存活时间</summary>
        public int TileCollideDefaultFragmentLife { get; set; } = 20;

        /// <summary>OnTileCollide 爆炸碎片跳过概率（0~1）</summary>
        public float TileCollideSkipChance { get; set; } = 0f;

        /// <summary>
        /// OnTileCollide 爆炸后是否销毁弹幕。
        /// true = 返回 true（销毁），false = 返回 false（继续存在）。
        /// </summary>
        public bool DestroyOnTileCollideExplosion { get; set; } = true;

        // 内部状态
        private bool _hasKillExploded = false;
        private Texture2D _resolvedTexture;

        public ExplosionKillBehavior() { }

        /// <summary>
        /// 使用颜色快速构造（仅 OnKill 爆炸）
        /// </summary>
        public ExplosionKillBehavior(Color colorStart, Color colorEnd, int count = 15, float speed = 4f)
        {
            ColorStart = colorStart;
            ColorEnd = colorEnd;
            KillCount = count;
            KillSpeed = speed;
        }

        public void OnSpawn(Projectile projectile, IEntitySource source)
        {
            _hasKillExploded = false;
            _resolvedTexture = TrailTexture ?? Terraria.GameContent.TextureAssets.Projectile[projectile.type].Value;
        }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            if (!ExplodeOnKill || _hasKillExploded) return;
            _hasKillExploded = true;

            SpawnExplosionInternal(
                projectile.Center,
                KillCount, KillSpeed, KillSizeMultiplier,
                KillFragmentLife ?? KillDefaultFragmentLife,
                KillSkipChance);
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch)
        {
            return true;
        }

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity)
        {
            if (!ExplodeOnTileCollide) return null;

            SpawnExplosionInternal(
                projectile.Center,
                TileCollideCount, TileCollideSpeed, TileCollideSizeMultiplier,
                TileCollideFragmentLife ?? TileCollideDefaultFragmentLife,
                TileCollideSkipChance);

            return DestroyOnTileCollideExplosion ? true : false;
        }

        /// <summary>
        /// 在指定位置生成爆炸效果（可被外部调用）。
        /// 使用 OnKill 参数。
        /// </summary>
        public void SpawnExplosion(Vector2 center)
        {
            SpawnExplosionInternal(
                center,
                KillCount, KillSpeed, KillSizeMultiplier,
                KillFragmentLife ?? KillDefaultFragmentLife,
                KillSkipChance);
        }

        /// <summary>
        /// 在指定位置生成爆炸效果（使用自定义参数）。
        /// </summary>
        public void SpawnExplosion(Vector2 center, int count, float speed, float sizeMultiplier, int life, float skipChance = 0f)
        {
            SpawnExplosionInternal(center, count, speed, sizeMultiplier, life, skipChance);
        }

        private void SpawnExplosionInternal(Vector2 center, int count, float speed, float sizeMultiplier, int life, float skipChance)
        {
            var manager = new LiquidTrailManager
            {
                TrailTexture = _resolvedTexture,
                ExplosionTexture = ExplosionTexture ?? _resolvedTexture,
                SizeMultiplier = sizeMultiplier,
                ColorStart = ColorStart,
                ColorEnd = ColorEnd,
                FragmentLife = life
            };

            manager.SpawnExplosion(center, count, speed, life, sizeMultiplier, skipChance);
        }
    }
}
