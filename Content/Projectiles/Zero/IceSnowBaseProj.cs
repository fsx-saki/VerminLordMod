using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using VerminLordMod.Common.BulletBehaviors;
using VerminLordMod.Content.DamageClasses;

namespace VerminLordMod.Content.Projectiles.Zero
{
    /// <summary>
    /// 冰道基础弹幕 — "向前凝结，后方碎裂"。
    ///
    /// 弹幕沿抛物线飞行，飞行途中每隔若干帧在当前位置"放置"一枚冰晶贴图。
    /// 冰晶原地不动，短暂停留后崩裂成冰碎片飞散。
    /// 整体模拟冰向前不断凝结、后方不断碎裂的效果。
    ///
    /// 行为组合：
    /// - GravityBehavior: 抛物线飞行
    /// - IceTrailBehavior: 冰系拖尾（十字星 + 雪片）
    /// - IceCrystalPlaceBehavior: 放置冰晶（贴图旋转 90°）+ 碎裂
    /// - KillOnContactBehavior: 碰到物块/敌人时销毁，触发爆炸
    /// - SuppressDrawBehavior: 隐藏默认贴图
    /// - OnKilled: 撞到敌人/物块时爆炸销毁，产生冰碎片
    /// </summary>
    public class IceSnowBaseProj : BaseBullet
    {
        private const int KillFragmentCount = 6;
        private const float KillFragmentSpeed = 4f;

        protected override void RegisterBehaviors()
        {
            // 重力再次降低为 1/3（0.08→0.027, 4.7→1.57），冰晶极缓慢飘落
            Behaviors.Add(new GravityBehavior(acceleration: 0.027f, maxFallSpeed: 1.57f)
            {
                AutoRotate = true,
                RotationOffset = MathHelper.PiOver2,
            });

            // 拖尾大小增加（StarSize 0.5→0.7, SnowflakeSize 0.25→0.35）
            Behaviors.Add(new IceTrailBehavior
            {
                MaxStars = 30,
                StarLife = 25,
                StarSpawnInterval = 3,
                StarSize = 0.7f,
                MaxSnowflakes = 80,
                SnowflakeLife = 22,
                SnowflakeSize = 0.35f,
                SnowflakeClusterSize = 4,
                SnowflakeSpawnChance = 0.6f,
                SnowflakeGravity = 0.08f,
                AutoDraw = true,
                SuppressDefaultDraw = false,
            });

            Behaviors.Add(new IceCrystalPlaceBehavior
            {
                PlaceInterval = 1,
                CrystalLife = 13,
                ShatterFragmentCount = 1,
                ShatterSpeed = 3f,
                FragmentSpawnChance = 0.3f,
                CrystalDrawScale = 0.7f,
                CrystalRotationOffset = MathHelper.PiOver2, // 冰晶贴图旋转 90°
            });

            // 碰到物块或敌人时销毁，触发 OnKill → OnKilled 爆碎片
            Behaviors.Add(new KillOnContactBehavior());

            Behaviors.Add(new SuppressDrawBehavior());
        }

        public override void SetDefaults()
        {
            // 主弹幕大小增加（14→20, scale 1→1.3）
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.scale = 1.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = ModContent.GetInstance<InsectDamageClass>();
            Projectile.aiStyle = -1;
        }

        protected override void OnAI()
        {
            Lighting.AddLight(Projectile.Center, 0.2f, 0.5f, 0.9f);
        }

        protected override void OnHit(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn, 120);
            target.AddBuff(BuffID.Slow, 180);
            target.AddBuff(BuffID.Chilled, 60);
        }

        protected override void OnKilled(int timeLeft)
        {
            int fragType = ModContent.ProjectileType<IceFragmentProj>();
            IEntitySource source = Main.player[Projectile.owner]?.GetSource_FromThis();

            for (int i = 0; i < KillFragmentCount; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(KillFragmentSpeed, KillFragmentSpeed);
                vel.Y -= Main.rand.NextFloat(1f, KillFragmentSpeed * 0.5f);

                Projectile.NewProjectile(
                    source,
                    Projectile.Center,
                    vel,
                    fragType,
                    0,
                    0f,
                    Projectile.owner
                );
            }
        }
    }
}