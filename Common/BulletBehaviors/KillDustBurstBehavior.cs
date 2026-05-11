using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace VerminLordMod.Common.BulletBehaviors
{
    /// <summary>
    /// 销毁粉尘爆发行为 — 弹幕销毁时生成多层 Dust 粒子爆发。
    ///
    /// 支持多层配置，每层可独立设置：
    /// - 粒子数量、类型、颜色、透明度
    /// - 速度模式（圆形散布 / 分量散布）
    /// - 大小范围、重力、散布半径
    ///
    /// 使用方式：
    /// <code>
    /// Behaviors.Add(new KillDustBurstBehavior
    /// {
    ///     Layers = new List&lt;KillDustBurstBehavior.DustBurstLayer&gt;
    ///     {
    ///         new() { Count = 20, NoGravity = true, SpeedMin = 2f, SpeedMax = 7f },
    ///         new() { Count = 8, NoGravity = false, UseCircularVelocity = false },
    ///     }
    /// });
    /// </code>
    /// </summary>
    public class KillDustBurstBehavior : IBulletBehavior
    {
        public string Name => "KillDustBurst";

        /// <summary>粉尘爆发层列表</summary>
        public List<DustBurstLayer> Layers { get; set; } = new();

        /// <summary>
        /// 单层粉尘爆发配置
        /// </summary>
        public class DustBurstLayer
        {
            /// <summary>粒子数量</summary>
            public int Count { get; set; } = 10;

            /// <summary>Dust 类型</summary>
            public int DustType { get; set; } = DustID.Water;

            /// <summary>粒子颜色</summary>
            public Color Color { get; set; } = Color.White;

            /// <summary>粒子透明度（0-255）</summary>
            public int Alpha { get; set; } = 0;

            /// <summary>粒子大小最小值</summary>
            public float ScaleMin { get; set; } = 0.5f;

            /// <summary>粒子大小最大值</summary>
            public float ScaleMax { get; set; } = 1.0f;

            /// <summary>是否无重力</summary>
            public bool NoGravity { get; set; } = true;

            /// <summary>生成位置散布半径（像素）</summary>
            public float SpreadRadius { get; set; } = 10f;

            /// <summary>速度模式：true=圆形散布（随机角度×速度），false=分量散布（X/Y 独立随机）</summary>
            public bool UseCircularVelocity { get; set; } = true;

            /// <summary>圆形散布最小速度</summary>
            public float SpeedMin { get; set; } = 2f;

            /// <summary>圆形散布最大速度</summary>
            public float SpeedMax { get; set; } = 6f;

            /// <summary>分量散布 X 最小值</summary>
            public float VelXMin { get; set; } = -3f;

            /// <summary>分量散布 X 最大值</summary>
            public float VelXMax { get; set; } = 3f;

            /// <summary>分量散布 Y 最小值</summary>
            public float VelYMin { get; set; } = -4f;

            /// <summary>分量散布 Y 最大值</summary>
            public float VelYMax { get; set; } = -1f;
        }

        public KillDustBurstBehavior() { }

        public void OnSpawn(Projectile projectile, IEntitySource source) { }

        public void Update(Projectile projectile) { }

        public void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) { }

        public void OnKill(Projectile projectile, int timeLeft)
        {
            foreach (var layer in Layers)
            {
                for (int i = 0; i < layer.Count; i++)
                {
                    Vector2 vel;
                    if (layer.UseCircularVelocity)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float speed = Main.rand.NextFloat(layer.SpeedMin, layer.SpeedMax);
                        vel = angle.ToRotationVector2() * speed;
                    }
                    else
                    {
                        vel = new Vector2(
                            Main.rand.NextFloat(layer.VelXMin, layer.VelXMax),
                            Main.rand.NextFloat(layer.VelYMin, layer.VelYMax)
                        );
                    }

                    Vector2 pos = projectile.Center + Main.rand.NextVector2Circular(layer.SpreadRadius, layer.SpreadRadius);
                    float scale = Main.rand.NextFloat(layer.ScaleMin, layer.ScaleMax);

                    Dust d = Dust.NewDustPerfect(pos, layer.DustType, vel, layer.Alpha, layer.Color, scale);
                    d.noGravity = layer.NoGravity;
                }
            }
        }

        public bool PreDraw(Projectile projectile, ref Color lightColor, SpriteBatch spriteBatch) => true;

        public bool? OnTileCollide(Projectile projectile, Vector2 oldVelocity) => null;
    }
}